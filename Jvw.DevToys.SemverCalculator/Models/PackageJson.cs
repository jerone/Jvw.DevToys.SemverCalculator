using System.Text.Json.Serialization;
using Jvw.DevToys.SemverCalculator.Converters;

namespace Jvw.DevToys.SemverCalculator.Models;

internal class PackageJson
{
    public required string Name { get; set; }

    [JsonConverter(typeof(DictionaryKeysListConverter))]
    public required List<string> Versions { get; set; }
}
