using System.ComponentModel;
using Jvw.DevToys.SemverCalculator.Components;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Components;

/// <summary>
/// Cheat sheet component tests.
/// </summary>
public class CheatSheetComponentTests
{
    [Fact]
    [Description("Snapshot cheat sheet NPM component.")]
    public async Task CheatSheetComponent_CheatSheetNpm()
    {
        // Act.
        var result = CheatSheetComponent.CheatSheetNpm();

        // Assert.
        await Verify(result);
    }

    [Fact]
    [Description("Snapshot cheat sheet NuGet component.")]
    public async Task CheatSheetComponent_CheatSheetNuGet()
    {
        // Act.
        var result = CheatSheetComponent.CheatSheetNuGet();

        // Assert.
        await Verify(result);
    }
}
