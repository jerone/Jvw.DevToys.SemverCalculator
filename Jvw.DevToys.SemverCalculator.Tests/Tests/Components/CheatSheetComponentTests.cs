using System.ComponentModel;
using Jvw.DevToys.SemverCalculator.Components;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Components;

/// <summary>
/// Cheat sheet component tests.
/// </summary>
public class CheatSheetComponentTests
{
    [Fact]
    [Description("Verify that the cheat sheet component has columns.")]
    public void CheatSheetComponent_HasColumns()
    {
        // Arrange.
        const int expectedCount = 3;

        // Act.
        var data = CheatSheetComponent.Columns;

        // Assert.
        Assert.Equal(expectedCount, data.Length);
    }

    [Fact]
    [Description("Verify that the cheat sheet component has rows.")]
    public async Task CheatSheetComponent_HasRows()
    {
        // Arrange.
        const int expectedCount = 18;

        // Act.
        var data = CheatSheetComponent.Rows;

        // Assert.
        Assert.Equal(expectedCount, data.Length);
        await Verify(data);
    }
}
