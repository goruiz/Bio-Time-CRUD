using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BioTime.DTOs;
using BioTime.DTOs.Employees;
using BioTime.Settings;
using Microsoft.Extensions.Options;

namespace BioTime.Services.Employees;

public class BioTimeService : IBioTimeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BioTimeSettings _settings;
    private readonly ILogger<BioTimeService> _logger;

    private string? _token;

    public BioTimeService(
        IHttpClientFactory httpClientFactory,
        IOptions<BioTimeSettings> settings,
        ILogger<BioTimeService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<PaginatedResponse<EmployeeDto>> GetEmployeesAsync(int page = 1, int pageSize = 10)
    {
        var response = await SendWithRetryAsync(HttpMethod.Get,
            $"personnel/api/employees/?page={page}&page_size={pageSize}");

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaginatedResponse<EmployeeDto>>(json)
            ?? throw new InvalidOperationException("No se pudo deserializar la respuesta de empleados.");

        return result;
    }

    public async Task<EmployeeDto> GetEmployeeByIdAsync(int id)
    {
        var response = await SendWithRetryAsync(HttpMethod.Get,
            $"personnel/api/employees/{id}/");

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<EmployeeDto>(json)
            ?? throw new InvalidOperationException($"No se pudo deserializar el empleado con ID {id}.");

        return result;
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto employee)
    {
        var response = await SendWithRetryAsync(HttpMethod.Post,
            "personnel/api/employees/", employee);

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<EmployeeDto>(json)
            ?? throw new InvalidOperationException("No se pudo deserializar la respuesta al crear empleado.");

        return result;
    }

    public async Task<EmployeeDto> UpdateEmployeeAsync(int id, UpdateEmployeeDto employee)
    {
        var response = await SendWithRetryAsync(HttpMethod.Put,
            $"personnel/api/employees/{id}/", employee);

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<EmployeeDto>(json)
            ?? throw new InvalidOperationException($"No se pudo deserializar la respuesta al actualizar empleado {id}.");

        return result;
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        await SendWithRetryAsync(HttpMethod.Delete,
            $"personnel/api/employees/{id}/");
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
