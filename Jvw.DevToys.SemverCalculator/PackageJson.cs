using System.Text.Json.Serialization;

namespace Jvw.DevToys.SemverCalculator;

public class PackageJson
{
    public required string Name { get; set; }

    [JsonConverter(typeof(DictionaryKeysListConverter))]
    public required List<string> Versions { get; set; }
}
