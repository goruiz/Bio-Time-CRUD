using System.Text.Json;
using System.Text.Json.Serialization;
using BioTime.DTOs.BioTime;

namespace BioTime.Converters;

public class DepartmentDtoConverter : JsonConverter<DepartmentDto?>
{
    public override DepartmentDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
        {
            return new DepartmentDto { Id = reader.GetInt32() };
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            return JsonSerializer.Deserialize<DepartmentDto>(ref reader);
        }

        throw new JsonException($"Unexpected token type for DepartmentDto: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, DepartmentDto? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            JsonSerializer.Serialize(writer, value);
    }
}
