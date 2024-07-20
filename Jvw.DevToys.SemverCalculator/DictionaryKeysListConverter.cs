using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jvw.DevToys.SemverCalculator;

internal sealed class DictionaryKeysListConverter : JsonConverter<List<string>>
{
    public override List<string> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var startDepth = reader.CurrentDepth;
        var list = new List<string>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == startDepth)
            {
                return list;
            }

            // Only care about properties from the same level object. No properties of children.
            if (
                reader.TokenType == JsonTokenType.PropertyName
                && reader.CurrentDepth == startDepth + 1
            )
            {
                var propertyName = reader.GetString();
                if (propertyName != null)
                {
                    list.Add(propertyName);
                }
            }
            else
            {
                reader.Skip();
            }
        }

        throw new JsonException();
    }

    public override void Write(
        Utf8JsonWriter writer,
        List<string> value,
        JsonSerializerOptions options
    )
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
