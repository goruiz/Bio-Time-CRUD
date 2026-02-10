using System.Text.Json;
using System.Text.Json.Serialization;

namespace BioTime.DTOs.Devices;

public class TerminalDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("sn")]
    public string Sn { get; set; } = string.Empty;

    [JsonPropertyName("alias")]
    public string Alias { get; set; } = string.Empty;

    [JsonPropertyName("ip_address")]
    public string IpAddress { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("terminal_name")]
    public string? TerminalName { get; set; }

    [JsonPropertyName("area")]
    [JsonConverter(typeof(FlexibleAreaConverter))]
    public JsonElement? Area { get; set; }
}

public class FlexibleAreaConverter : JsonConverter<JsonElement?>
{
    public override JsonElement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonElement.ParseValue(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, JsonElement? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            value.Value.WriteTo(writer);
        else
            writer.WriteNullValue();
    }
}
