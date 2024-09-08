using System.ComponentModel;
using Jvw.DevToys.SemverCalculator.Models;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Models;

/// <summary>
/// Settings tests.
/// </summary>
public class SettingsTests
{
    [Fact]
    [Description("Settings has HTTP agreement info-bar closed state.")]
    public async Task Settings_HasHttpAgreementClosed()
    {
        // Act.
        var defaultValue = Settings.HttpAgreementClosed;

        // Assert.
        await Verify(defaultValue);
    }
}
