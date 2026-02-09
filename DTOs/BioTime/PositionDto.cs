using System.Text.Json.Serialization;

namespace BioTime.DTOs.BioTime;

public class PositionDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("position_code")]
    public string PositionCode { get; set; } = string.Empty;

    [JsonPropertyName("position_name")]
    public string PositionName { get; set; } = string.Empty;
}
