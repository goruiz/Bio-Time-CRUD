using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using BioTime.DTOs.BioTime;
using BioTime.Settings;
using Microsoft.Extensions.Options;

namespace BioTime.Services;

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
        var client = await GetAuthenticatedClientAsync();

        var response = await client.GetAsync($"personnel/api/employees/?page={page}&page_size={pageSize}");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Token expirado. Reintentando login...");
            _token = null;
            client = await GetAuthenticatedClientAsync();
            response = await client.GetAsync($"personnel/api/employees/?page={page}&page_size={pageSize}");
        }

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaginatedResponse<EmployeeDto>>(json)
            ?? throw new InvalidOperationException("No se pudo deserializar la respuesta de empleados.");

        return result;
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
