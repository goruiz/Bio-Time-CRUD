using System.Text.Json;
using System.Text.Json.Serialization;
using BioTime.DTOs.Employees;

namespace BioTime.Converters.Employees;

public class PositionDtoConverter : JsonConverter<PositionDto?>
{
    public override PositionDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
        {
            return new PositionDto { Id = reader.GetInt32() };
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            return JsonSerializer.Deserialize<PositionDto>(ref reader);
        }

        throw new JsonException($"Unexpected token type for PositionDto: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, PositionDto? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            JsonSerializer.Serialize(writer, value);
    }
}
