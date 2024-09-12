using DevToys.Api;
using Semver;
using static DevToys.Api.GUI;

namespace Jvw.DevToys.SemverCalculator.Services;

/// <summary>
/// Service to keep track of all package versions and match them with a range.
/// </summary>
/// <param name="clipboard">Clipboard.</param>
internal class VersionService(IClipboard clipboard)
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
    /// Try to parse range and store it.
    /// </summary>
    /// <param name="value">Range value.</param>
    /// <returns>Whether value is valid range.</returns>
    public bool TryParseRange(string value)
    {
        var valid = SemVersionRange.TryParseNpm(value, true, out _range);
        if (!valid)
            _range = null;
        return valid;
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
            var element = Button()
                .Text(text)
                .OnClick(() =>
                {
                    clipboard.SetClipboardTextAsync(version.ToString()).Forget();
                });
            list.Add(element);
        }

        return list;
    }
}
