using System.Text.Json;
using System.Text.Json.Serialization;
using BioTime.DTOs.Areas;

namespace BioTime.Converters.Areas;

public class ParentAreaDtoConverter : JsonConverter<ParentAreaDto?>
{
    public override ParentAreaDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
        {
            return new ParentAreaDto { Id = reader.GetInt32() };
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            return JsonSerializer.Deserialize<ParentAreaDto>(ref reader);
        }

        throw new JsonException($"Unexpected token type for ParentAreaDto: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, ParentAreaDto? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            JsonSerializer.Serialize(writer, value);
    }
}
