using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jvw.DevToys.SemverCalculator.Converters;

/// <summary>
/// Converts a dictionary to a list of keys.
/// </summary>
internal sealed class DictionaryToKeysListConverter : JsonConverter<List<string>>
{
    /// <inheritdoc cref="JsonConverter{List}.Read" />
    /// <exception cref="JsonException">Throws exception when token-type is not start object.</exception>
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
                break;
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
                continue;
            }

            reader.Skip();
        }

        return list;
    }

    /// <exception cref="NotImplementedException">Writing is not supported.</exception>
    public override void Write(
        Utf8JsonWriter writer,
        List<string> value,
        JsonSerializerOptions options
    )
    {
        throw new NotImplementedException("Writing is not supported.");
    }
}
