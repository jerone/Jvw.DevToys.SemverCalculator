using DevToys.Api;
using Semver;
using static DevToys.Api.GUI;

namespace Jvw.DevToys.SemverCalculator.Services;

internal class VersionService(IClipboard clipboard)
{
    private List<SemVersion> _versions = [];
    private SemVersionRange? _range;

    public void SetVersions(List<string> versions)
    {
        _versions = versions.Select(v => SemVersion.Parse(v, SemVersionStyles.Strict)).ToList();
        _versions.Sort(SemVersion.SortOrderComparer);
    }

    public bool TryParseRange(string value)
    {
        var valid = SemVersionRange.TryParseNpm(value, true, out _range);
        if (!valid)
            _range = null;
        return valid;
    }

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
