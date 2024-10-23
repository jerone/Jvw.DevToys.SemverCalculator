using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Enums;

namespace Jvw.DevToys.SemverCalculator.Models;

/// <summary>
/// Settings for the extension.
/// </summary>
internal static class Settings
{
    /// <summary>
    /// Setting to keep track if user has closed the HTTP agreement info-bar.
    /// </summary>
    internal static readonly SettingDefinition<bool> HttpAgreementClosed =
        new(name: "Settings." + nameof(HttpAgreementClosed), defaultValue: false);

    /// <summary>
    /// Setting to keep track of the selected package manager. Default is NPM.
    /// </summary>
    internal static readonly SettingDefinition<PackageManager> PackageManager =
        new(name: "Settings." + nameof(PackageManager), defaultValue: Enums.PackageManager.Npm);

    /// <summary>
    /// Setting to keep track whether to include pre-releases. Default is false.
    /// </summary>
    internal static readonly SettingDefinition<bool> IncludePreReleases =
        new(name: "Settings." + nameof(IncludePreReleases), defaultValue: false);
}
