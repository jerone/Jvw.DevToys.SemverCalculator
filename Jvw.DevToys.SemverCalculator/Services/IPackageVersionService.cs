using Jvw.DevToys.SemverCalculator.Enums;

namespace Jvw.DevToys.SemverCalculator.Services;

/// <summary>
/// Interface for package version services.
/// </summary>
internal interface IPackageVersionService
{
    /// <summary>
    /// Package manager name.
    /// </summary>
    PackageManager PackageManager { get; }

    /// <summary>
    /// Store all versions of a package.
    /// </summary>
    /// <param name="versions">Package versions.</param>
    void SetVersions(List<string> versions);

    /// <summary>
    /// Get list of versions and whether they match the range.
    /// </summary>
    /// <param name="includePreReleases">Include pre-releases during matching.</param>
    /// <returns>List of versions and whether they match the range.</returns>
    IEnumerable<(string version, bool match)> GetVersions(bool includePreReleases);

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
}
