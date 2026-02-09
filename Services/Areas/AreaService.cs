using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BioTime.DTOs;
using BioTime.DTOs.Areas;
using BioTime.Settings;
using Microsoft.Extensions.Options;

namespace BioTime.Services.Areas;

public class AreaService : IAreaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BioTimeSettings _settings;
    private readonly ILogger<AreaService> _logger;

    private string? _token;

    public AreaService(
        IHttpClientFactory httpClientFactory,
        IOptions<BioTimeSettings> settings,
        ILogger<AreaService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<PaginatedResponse<AreaDto>> GetAreasAsync(int page = 1, int pageSize = 10)
    {
        var response = await SendWithRetryAsync(HttpMethod.Get,
            $"personnel/api/areas/?page={page}&page_size={pageSize}");

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaginatedResponse<AreaDto>>(json)
            ?? throw new InvalidOperationException("No se pudo deserializar la respuesta de áreas.");

        return result;
    }

    public async Task<AreaDto> GetAreaByIdAsync(int id)
    {
        var response = await SendWithRetryAsync(HttpMethod.Get,
            $"personnel/api/areas/{id}/");

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AreaDto>(json)
            ?? throw new InvalidOperationException($"No se pudo deserializar el área con ID {id}.");

        return result;
    }

    public async Task<AreaDto> CreateAreaAsync(CreateAreaDto area)
    {
        var response = await SendWithRetryAsync(HttpMethod.Post,
            "personnel/api/areas/", area);

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AreaDto>(json)
            ?? throw new InvalidOperationException("No se pudo deserializar la respuesta al crear área.");

        return result;
    }

    public async Task<AreaDto> UpdateAreaAsync(int id, UpdateAreaDto area)
    {
        var response = await SendWithRetryAsync(HttpMethod.Put,
            $"personnel/api/areas/{id}/", area);

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<AreaDto>(json)
            ?? throw new InvalidOperationException($"No se pudo deserializar la respuesta al actualizar área {id}.");

        return result;
    }

    public async Task DeleteAreaAsync(int id)
    {
        await SendWithRetryAsync(HttpMethod.Delete,
            $"personnel/api/areas/{id}/");
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

        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (contentType != null && !contentType.Contains("json"))
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("BioTime devolvió contenido no-JSON. ContentType: {ContentType}, Body: {Body}", contentType, responseBody);
            throw new HttpRequestException($"BioTime devolvió contenido no-JSON ({contentType}) para {url}");
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
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

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

        var loginRequest = new LoginRequest
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

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>()
            ?? throw new InvalidOperationException("Respuesta de login vacía.");

        _token = loginResponse.Token;
        _logger.LogInformation("Autenticación exitosa contra BioTime.");
    }
}
