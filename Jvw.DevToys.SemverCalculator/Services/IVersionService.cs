using DevToys.Api;

namespace Jvw.DevToys.SemverCalculator.Services;

/// <summary>
/// Service to keep track of all package versions and match them with a range.
/// </summary>
internal interface IVersionService
{
    /// <summary>
    /// Store all versions of a package.
    /// </summary>
    /// <param name="versions">Package versions.</param>
    void SetVersions(List<string> versions);

    /// <summary>
    /// Try to parse range, store it and return whether it is valid.
    /// </summary>
    /// <param name="value">Range value.</param>
    /// <returns>Whether value is valid range.</returns>
    bool TryParseRange(string value);

    /// <summary>
    /// Check if value is a valid range.
    /// </summary>
    /// <param name="value">Range value.</param>
    /// <returns>Whether value is valid range.</returns>
    bool IsValidRange(string value);

    /// <summary>
    /// Create UI labels for all versions that match the range.
    /// </summary>
    /// <param name="includePreReleases">Include pre-releases during matching.</param>
    /// <returns>List of UI labels.</returns>
    List<IUIElement> MatchVersions(bool includePreReleases);
}
