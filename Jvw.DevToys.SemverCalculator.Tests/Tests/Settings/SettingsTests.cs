using System.ComponentModel;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Settings;

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
        var defaultValue = Models.Settings.HttpAgreementClosed;

        // Assert.
        await Verify(defaultValue);
    }
}
