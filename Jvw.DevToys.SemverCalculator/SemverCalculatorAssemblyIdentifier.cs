using System.ComponentModel.Composition;
using DevToys.Api;

namespace Jvw.DevToys.SemverCalculator;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(SemverCalculatorAssemblyIdentifier))]
internal sealed class SemverCalculatorAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        return new ValueTask<FontDefinition[]>(Array.Empty<FontDefinition>());
    }
}
