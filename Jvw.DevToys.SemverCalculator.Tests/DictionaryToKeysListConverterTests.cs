using System.Text.Json;
using Jvw.DevToys.SemverCalculator.Converters;

namespace Jvw.DevToys.SemverCalculator.Tests;

public class DictionaryToKeysListConverterTests
{
    [Fact]
    public void Read_Integration_WithJsonObject_ReturnListOfKeys()
    {
        // Arrange.
        const string json =
            "{\"key1\":\"value1\",\"key2\":\"value2\",\"key3\":{\"sub-key\":\"value3\"}}";
        var converter = new DictionaryToKeysListConverter();
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);

        // Act.
        var result = JsonSerializer.Deserialize<List<string>>(json, options);

        // Assert.
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(["key1", "key2", "key3"], result);
    }

    [Fact]
    public void Read_Implementation_WithNoStartObject_ThrowsJsonException()
    {
        // Arrange.
        var converter = new DictionaryToKeysListConverter();
        var options = new JsonSerializerOptions();

        // Act.
        void Result()
        {
            var reader = new Utf8JsonReader(Array.Empty<byte>());
            converter.Read(ref reader, typeof(List<string>), options);
        }

        // Assert.
        Assert.Throws<JsonException>(Result);
    }

    [Fact]
    public void Write_Integration_WithAnyValues_ThrowsNotImplementedException()
    {
        // Arrange.
        var list = new List<string> { "key1", "key2", "key3" };
        var converter = new DictionaryToKeysListConverter();
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);

        // Act.
        void Result() => JsonSerializer.Serialize(list, options);

        // Assert.
        Assert.Throws<NotImplementedException>(Result);
    }
}
