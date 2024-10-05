using System.ComponentModel.Composition;
using Jvw.DevToys.SemverCalculator.Enums;
using Semver;

namespace Jvw.DevToys.SemverCalculator.Services;

/// <summary>
/// Service to keep track of all NPM package versions and match them with a range.
/// </summary>
[Export(typeof(IPackageVersionService))]
internal class NpmVersionService : IPackageVersionService
{
    private List<SemVersion> _versions = [];
    private SemVersionRange? _range;

    /// <inheritdoc cref="IPackageVersionService.PackageManager" />
    public PackageManager PackageManager => PackageManager.Npm;

    /// <inheritdoc cref="IPackageVersionService.SetVersions" />
    public void SetVersions(List<string> versions)
    {
        _versions = versions.Select(v => SemVersion.Parse(v, SemVersionStyles.Strict)).ToList();
        _versions.Sort(SemVersion.SortOrderComparer);
    }

    /// <inheritdoc cref="IPackageVersionService.GetVersions" />
    public IEnumerable<(string version, bool match)> GetVersions(bool includePreReleases)
    {
        foreach (var version in _versions)
        {
            if (!includePreReleases && version.IsPrerelease)
            {
                continue;
            }

            yield return (version.ToString(), _range != null && _range.Contains(version));
        }
    }

    /// <inheritdoc cref="IPackageVersionService.TryParseRange" />
    public bool TryParseRange(string value)
    {
        var valid = TryParseRange(value, out _range);
        if (!valid)
            _range = null;
        return valid;
    }

    /// <inheritdoc cref="IPackageVersionService.IsValidRange" />
    public bool IsValidRange(string value)
    {
        return TryParseRange(value, out _);
    }

    /// <summary>
    /// Try to parse range.
    /// </summary>
    /// <param name="value">Range value.</param>
    /// <param name="versionRange">Parsed version range.</param>
    /// <returns>Whether value is valid range.</returns>
    private static bool TryParseRange(string value, out SemVersionRange versionRange)
    {
        return SemVersionRange.TryParseNpm(value, true, out versionRange);
    }
}
