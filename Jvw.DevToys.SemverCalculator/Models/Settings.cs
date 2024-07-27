using DevToys.Api;

namespace Jvw.DevToys.SemverCalculator.Models;

/// <summary>
/// Settings for the extension.
/// </summary>
internal static class Settings
{
    /// <summary>
    /// When user closes the HTTP agreement info-bar, this setting is set to true.
    /// </summary>
    public static SettingDefinition<bool> HttpAgreementClosed =
        new(name: "Settings." + nameof(HttpAgreementClosed), defaultValue: false);
}
