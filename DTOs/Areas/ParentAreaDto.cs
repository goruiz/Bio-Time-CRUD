using System.Text.Json.Serialization;

namespace BioTime.DTOs.Areas;

public class ParentAreaDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("area_code")]
    public string AreaCode { get; set; } = string.Empty;

    [JsonPropertyName("area_name")]
    public string AreaName { get; set; } = string.Empty;

    [JsonPropertyName("parent_area")]
    public int? ParentArea { get; set; }
}
