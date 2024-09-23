using System.ComponentModel;
using Jvw.DevToys.SemverCalculator.Resources;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Resources;

/// <summary>
/// Resource assembly identifier tests.
/// </summary>
public class ResourceAssemblyIdentifierTests
{
    [Fact]
    [Description("Verify that GetFontDefinitionsAsync returns an empty array.")]
    public async Task GetFontDefinitionsAsync_ReturnsEmptyArray()
    {
        // Arrange.
        var sut = new ResourceAssemblyIdentifier();

        // Act.
        var results = await sut.GetFontDefinitionsAsync();

        // Assert.
        Assert.Empty(results);
    }
}
