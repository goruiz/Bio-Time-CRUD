using System.Text.Json.Serialization;

namespace BioTime.DTOs.BioTime;

public class LoginResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}
