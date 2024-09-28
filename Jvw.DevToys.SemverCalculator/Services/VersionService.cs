using System.ComponentModel.Composition;
using DevToys.Api;
using Semver;
using static DevToys.Api.GUI;

namespace Jvw.DevToys.SemverCalculator.Services;

/// <summary>
/// Service to keep track of all package versions and match them with a range.
/// </summary>
/// <param name="clipboard">Clipboard.</param>
[Export(typeof(IVersionService))]
[method: ImportingConstructor]
internal class VersionService(IClipboard clipboard) : IVersionService
{
    private List<SemVersion> _versions = [];
    private SemVersionRange? _range;

    /// <summary>
    /// Store all versions of a package.
    /// </summary>
    /// <param name="versions">Package versions.</param>
    public void SetVersions(List<string> versions)
    {
        _versions = versions.Select(v => SemVersion.Parse(v, SemVersionStyles.Strict)).ToList();
        _versions.Sort(SemVersion.SortOrderComparer);
    }

    /// <summary>
    /// Try to parse range, store it and return whether it is valid.
    /// </summary>
    /// <param name="value">Range value.</param>
    /// <returns>Whether value is valid range.</returns>
    public bool TryParseRange(string value)
    {
        var valid = TryParseRange(value, out _range);
        if (!valid)
            _range = null;
        return valid;
    }

    /// <summary>
    /// Check if value is a valid range.
    /// </summary>
    /// <param name="value">Range value.</param>
    /// <returns>Whether value is valid range.</returns>
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

    /// <summary>
    /// Create UI labels for all versions that match the range.
    /// </summary>
    /// <param name="includePreReleases">Include pre-releases during matching.</param>
    /// <returns>List of UI labels.</returns>
    public List<IUIElement> MatchVersions(bool includePreReleases)
    {
        if (_versions.Count == 0)
        {
            return [];
        }

        var list = new List<IUIElement>();

        foreach (var version in _versions)
        {
            if (!includePreReleases && version.IsPrerelease)
            {
                continue;
            }

            var match = _range != null && _range.Contains(version);
            var text = $"{(match ? "✅" : "🔳")} {version}";
            var element = Button().Text(text).OnClick(OnClickHandler(version));
            list.Add(element);
        }

        return list;

        Action OnClickHandler(SemVersion version) =>
            () =>
            {
                clipboard.SetClipboardTextAsync(version.ToString()).Forget();
            };
    }
}
