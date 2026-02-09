using System.Text.Json.Serialization;

namespace BioTime.DTOs;

public class PaginatedResponse<T>
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }

    [JsonPropertyName("previous")]
    public string? Previous { get; set; }

    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = [];
}
