using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Jvw.DevToys.SemverCalculator.Converters;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Converters;

/// <summary>
/// Unit tests for the <see cref="DictionaryToKeysListConverter"/> class.
/// </summary>
public class DictionaryToKeysListConverterTests
{
    [Fact]
    [Description("Extract keys from JSON object.")]
    public void Read_WithJsonObject_ReturnListOfKeys()
    {
        // Arrange.
        const string json = """
                            {
                                "key1": "value1",
                                "key2": "value2",
                                "key3": {
                                    "sub-key": "value3"
                                }
                            }
                            """;
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
    [Description("Throw JSON exception when object has no start token.")]
    public void Read_WithNoStartToken_ThrowsJsonException()
    {
        // Arrange.
        var converter = new DictionaryToKeysListConverter();
        var options = new JsonSerializerOptions();

        // Act.
        [ExcludeFromCodeCoverage(Justification = "Used in unit-tests only.")]
        void Result()
        {
            var reader = new Utf8JsonReader(Array.Empty<byte>());
            converter.Read(ref reader, typeof(List<string>), options);
        }

        // Assert.
        Assert.Throws<JsonException>(Result);
    }

    [Fact]
    [Description("Writing is not implemented and throws exception.")]
    public void Write_WithAnyValues_ThrowsNotImplementedException()
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
