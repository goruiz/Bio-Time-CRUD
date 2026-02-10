using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BioTime.DTOs;
using BioTime.DTOs.Devices;
using BioTime.Settings;
using Microsoft.Extensions.Options;

namespace BioTime.Services.Devices;

public class DeviceService : IDeviceService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BioTimeSettings _settings;
    private readonly ILogger<DeviceService> _logger;

    private string? _token;

    public DeviceService(
        IHttpClientFactory httpClientFactory,
        IOptions<BioTimeSettings> settings,
        ILogger<DeviceService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<PaginatedResponse<TerminalDto>> GetTerminalsAsync(int page = 1, int pageSize = 10)
    {
        var response = await SendWithRetryAsync(HttpMethod.Get,
            $"iclock/api/terminals/?page={page}&page_size={pageSize}");

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaginatedResponse<TerminalDto>>(json)
            ?? throw new InvalidOperationException("No se pudo deserializar la respuesta de terminales.");

        return result;
    }

    public async Task<TerminalDto> GetTerminalByIdAsync(int id)
    {
        var response = await SendWithRetryAsync(HttpMethod.Get,
            $"iclock/api/terminals/{id}/");

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TerminalDto>(json)
            ?? throw new InvalidOperationException($"No se pudo deserializar el terminal con ID {id}.");

        return result;
    }

    private static readonly string[] SyncEndpointTemplates =
    [
        "iclock/api/terminals/{0}/sync/",
        "iclock/api/terminals/{0}/sync_user/",
        "iclock/api/terminals/{0}/sync_transaction/",
        "personnel/api/terminal/{0}/sync/",
    ];

    public async Task<List<SyncResultDto>> SyncAllTerminalsAsync()
    {
        var results = new List<SyncResultDto>();
        var page = 1;
        const int pageSize = 50;

        do
        {
            var terminalsPage = await GetTerminalsAsync(page, pageSize);

            foreach (var terminal in terminalsPage.Data)
            {
                var syncResult = await SyncTerminalAsync(terminal.Id, terminal.Sn);
                results.Add(syncResult);
            }

            page++;

            if (terminalsPage.Next == null)
                break;

        } while (true);

        _logger.LogInformation("Sincronización completada. {Count} terminales procesados.", results.Count);
        return results;
    }

    public async Task<SyncResultDto> SyncTerminalBySnAsync(string sn)
    {
        var terminalsPage = await GetTerminalsAsync(1, 100);
        var terminal = terminalsPage.Data.FirstOrDefault(t => t.Sn == sn);

        if (terminal == null)
            return new SyncResultDto { Success = false, TerminalSn = sn, Message = $"Terminal con SN={sn} no encontrado." };

        return await SyncTerminalAsync(terminal.Id, terminal.Sn);
    }

    private async Task<SyncResultDto> SyncTerminalAsync(int terminalId, string terminalSn)
    {
        foreach (var template in SyncEndpointTemplates)
        {
            var urlById = string.Format(template, terminalId);
            var result = await TrySyncEndpointAsync(urlById, terminalSn);
            if (result != null) return result;

            var urlBySn = string.Format(template, terminalSn);
            if (urlBySn != urlById)
            {
                result = await TrySyncEndpointAsync(urlBySn, terminalSn);
                if (result != null) return result;
            }
        }

        return new SyncResultDto
        {
            Success = false,
            TerminalSn = terminalSn,
            Message = "Ningún endpoint de sincronización respondió correctamente."
        };
    }

    private async Task<SyncResultDto?> TrySyncEndpointAsync(string url, string terminalSn)
    {
        try
        {
            var client = await GetAuthenticatedClientAsync();
            var response = await SendRequestAsync(client, HttpMethod.Post, url, null);
            var body = await response.Content.ReadAsStringAsync();

            var isHtml = body.TrimStart().StartsWith('<');

            if (response.IsSuccessStatusCode && !isHtml)
            {
                return new SyncResultDto
                {
                    Success = true,
                    TerminalSn = terminalSn,
                    Message = $"Sincronización exitosa via {url}"
                };
            }
        }
        catch
        {
            // Endpoint no disponible, probar siguiente
        }

        return null;
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(HttpMethod method, string url, object? body = null)
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await SendRequestAsync(client, method, url, body);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Token expirado. Reintentando login...");
            _token = null;
            client = await GetAuthenticatedClientAsync();
            response = await SendRequestAsync(client, method, url, body);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error de BioTime. Status: {Status}, Body: {Body}", response.StatusCode, errorBody);
            throw new HttpRequestException($"BioTime respondió {(int)response.StatusCode}: {errorBody}");
        }

        return response;
    }

    private static readonly JsonSerializerOptions _snakeCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private static async Task<HttpResponseMessage> SendRequestAsync(HttpClient client, HttpMethod method, string url, object? body)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, _snakeCaseOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return await client.SendAsync(request);
    }

    private async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        if (string.IsNullOrEmpty(_token))
            await LoginAsync();

        var client = _httpClientFactory.CreateClient("BioTime");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", _token);
        return client;
    }

    private async Task LoginAsync()
    {
        _logger.LogInformation("Autenticando contra BioTime...");

        var client = _httpClientFactory.CreateClient("BioTime");

        var loginRequest = new DTOs.LoginRequest
        {
            Username = _settings.Username,
            Password = _settings.Password
        };

        var response = await client.PostAsJsonAsync("jwt-api-token-auth/", loginRequest);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("Login fallido contra BioTime. Status: {Status}, Body: {Body}",
                response.StatusCode, body);
            throw new HttpRequestException($"Error de autenticación contra BioTime: {response.StatusCode}");
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<DTOs.LoginResponse>()
            ?? throw new InvalidOperationException("Respuesta de login vacía.");

        _token = loginResponse.Token;
        _logger.LogInformation("Autenticación exitosa contra BioTime.");
    }
}
