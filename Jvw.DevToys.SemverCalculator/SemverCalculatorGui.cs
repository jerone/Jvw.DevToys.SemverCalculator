using System.ComponentModel.Composition;
using System.Net;
using System.Text.Json;
using DevToys.Api;
using Microsoft.Extensions.Logging;
using Semver;
using static DevToys.Api.GUI;

namespace Jvw.DevToys.SemverCalculator;

[Export(typeof(IGuiTool))]
[Name("Jvw.DevToys.SemverCalculator")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uf20a',
    GroupName = PredefinedCommonToolGroupNames.Testers,
    ResourceManagerAssemblyIdentifier = nameof(SemverCalculatorAssemblyIdentifier),
    ResourceManagerBaseName = "Jvw.DevToys.SemverCalculator." + nameof(SemverCalculatorResources),
    ShortDisplayTitleResourceName = nameof(SemverCalculatorResources.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(SemverCalculatorResources.LongDisplayTitle),
    DescriptionResourceName = nameof(SemverCalculatorResources.Description),
    AccessibleNameResourceName = nameof(SemverCalculatorResources.AccessibleName)
)]
internal sealed class SemverCalculatorGui : IGuiTool
{
    private readonly ILogger _logger;

    private readonly IUISingleLineTextInput _packageNameInput = SingleLineTextInput();
    private readonly IUIInfoBar _packageNameWarningBar = InfoBar();
    private readonly IUISingleLineTextInput _versionRangeInput = SingleLineTextInput();
    private readonly IUIInfoBar _versionRangeWarningBar = InfoBar();
    private readonly IUIWrap _versionsList = Wrap();
    private readonly IUIProgressRing _progressRing = ProgressRing();

    private SemVersionRange? _range;
    private List<string>? _versions;

    public SemverCalculatorGui()
    {
        _logger = this.Log();

#if DEBUG
        _packageNameInput.Text("api");
        _versionRangeInput.Text("^3 || 6.1");
#endif
    }

    public UIToolView View =>
        new(
            isScrollable: true,
            Grid()
                .ColumnLargeSpacing()
                .RowLargeSpacing()
                .Rows(
                    (GridRow.Settings, Auto),
                    (GridRow.Results, new UIGridLength(1, UIGridUnitType.Fraction))
                )
                .Columns((GridColumn.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))
                .Cells(
                    Cell(
                        GridRow.Settings,
                        GridColumn.Stretch,
                        Stack()
                            .Vertical()
                            .WithChildren(
                                _packageNameInput
                                    .Title("NPM package name")
                                    .CommandBarExtraContent(
                                        Button()
                                            .AccentAppearance()
                                            .Text("Load package versions")
                                            .OnClick(OnLoadPackageClick)
                                    ),
                                _packageNameWarningBar.Warning().ShowIcon().NonClosable(),
                                _versionRangeInput
                                    .Title("Version range")
                                    .OnTextChanged(OnVersionRangeChange),
                                _versionRangeWarningBar.Warning().ShowIcon().NonClosable()
                            )
                    ),
                    Cell(
                        GridRow.Results,
                        GridColumn.Stretch,
                        Card(
                            Stack()
                                .Vertical()
                                .AlignHorizontally(UIHorizontalAlignment.Center)
                                .WithChildren(_progressRing, _versionsList.LargeSpacing())
                        )
                    )
                )
        );

    private async ValueTask OnLoadPackageClick()
    {
        _packageNameWarningBar.Close();
        _progressRing.StartIndeterminateProgress().Show();
        _versionsList.WithChildren();

        if (string.IsNullOrWhiteSpace(_packageNameInput.Text))
        {
            _packageNameWarningBar.Description("Package name is required.").Open();
            _progressRing.StopIndeterminateProgress().Hide();
            return;
        }

        var package = await FetchPackage(_packageNameInput.Text);
        if (package == null)
        {
            // TODO: distinct between network error and package not found.
            _packageNameWarningBar.Description("Failed to fetch package.").Open();
            _progressRing.StopIndeterminateProgress().Hide();
            return;
        }

        _versions = package.Versions;

        await OnVersionRangeChange(_versionRangeInput.Text);

        var list = MatchVersions();

        _versionsList.WithChildren([.. list]);
        _progressRing.StopIndeterminateProgress().Hide();
    }

    private ValueTask OnVersionRangeChange(string value)
    {
        _versionRangeWarningBar.Close();

        if (string.IsNullOrWhiteSpace(value))
        {
            _range = null;
        }
        else if (SemVersionRange.TryParseNpm(value, true, out _range))
        {
            _progressRing.StartIndeterminateProgress().Show();

            var list = MatchVersions();

            _versionsList.WithChildren([.. list]);
            _progressRing.StopIndeterminateProgress().Hide();
        }
        else
        {
            _versionRangeWarningBar.Description("Version range appears to be not valid.").Open();
            _range = null;
        }

        return ValueTask.CompletedTask;
    }

    private List<IUIElement> MatchVersions()
    {
        if (_versions == null || _versions.Count == 0)
            return [];

        var list = new List<IUIElement>();

        var versions = _versions.Select(v => SemVersion.Parse(v, SemVersionStyles.Strict)).ToList();
        versions.Sort(SemVersion.SortOrderComparer);

        foreach (var version in versions)
        {
            var match = _range != null && _range.Contains(version);
            var text = $"{(match ? "✅" : "🔳")} {version}";
            var element = Button().Text(text);
            list.Add(element);
        }

        return list;
    }

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        // Not implemented.
    }

    private async Task<PackageJson?> FetchPackage(string packageName)
    {
        _logger.LogInformation($"Fetching package \"{packageName}\"...");
        try
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.npm.install-vl+json");

            var response = await client.GetAsync($"https://registry.npmjs.org/{packageName}/");

            response.EnsureSuccessStatusCode();
            _logger.LogInformation($"Fetched packages \"{packageName}\".");

            var contentStream = await response.Content.ReadAsStreamAsync();

            var result = await JsonSerializer.DeserializeAsync<PackageJson>(
                contentStream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result;
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning($"Package \"{packageName}\" not found.");
            Console.WriteLine(e.Message);
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to fetch package \"{packageName}\".");
            Console.WriteLine(e.Message);
            return null;
        }
    }
}
