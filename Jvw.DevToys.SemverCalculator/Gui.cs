using System.ComponentModel.Composition;
using System.Net;
using System.Text.Json;
using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Enums;
using Jvw.DevToys.SemverCalculator.Models;
using Jvw.DevToys.SemverCalculator.Resources;
using Microsoft.Extensions.Logging;
using Semver;
using static DevToys.Api.GUI;
using R = Jvw.DevToys.SemverCalculator.Resources.Resources;

namespace Jvw.DevToys.SemverCalculator;

[Export(typeof(IGuiTool))]
[Name("Jvw.DevToys.SemverCalculator")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uf20a',
    GroupName = PredefinedCommonToolGroupNames.Testers,
    ResourceManagerAssemblyIdentifier = nameof(ResourceAssemblyIdentifier),
    ResourceManagerBaseName = "Jvw.DevToys.SemverCalculator.Resources." + nameof(Resources),
#if DEBUG
    ShortDisplayTitleResourceName = nameof(R.ShortDisplayTitleResourceNameDebug),
    LongDisplayTitleResourceName = nameof(R.LongDisplayTitleResourceNameDebug),
#else
    ShortDisplayTitleResourceName = nameof(R.ShortDisplayTitleResourceName),
    LongDisplayTitleResourceName = nameof(R.LongDisplayTitleResourceName),
#endif
    DescriptionResourceName = nameof(R.DescriptionResourceName),
    AccessibleNameResourceName = nameof(R.AccessibleNameResourceName)
)]
internal sealed class Gui : IGuiTool
{
    private readonly IClipboard _clipboard;
    private readonly ISettingsProvider _settingsProvider;
    private readonly ILogger _logger;

    private readonly IUISingleLineTextInput _packageNameInput = SingleLineTextInput();
    private readonly IUIInfoBar _packageNameWarningBar = InfoBar();
    private readonly IUISingleLineTextInput _versionRangeInput = SingleLineTextInput();
    private readonly IUIInfoBar _versionRangeWarningBar = InfoBar();
    private readonly IUIWrap _versionsList = Wrap();
    private readonly IUIProgressRing _progressRing = ProgressRing();

    private List<string>? _versions;
    private bool _includePreReleases;
    private SemVersionRange? _range;

    [ImportingConstructor]
    public Gui(IClipboard clipboard, ISettingsProvider settingsProvider)
    {
        _clipboard = clipboard;
        _settingsProvider = settingsProvider;
        _logger = this.Log();

#if DEBUG
        _packageNameInput.Text("api");
        _versionRangeInput.Text("1 || ~3.2 || ^5.0.5");
#endif
    }

    public UIToolView View
    {
        get
        {
            var httpAgreementClosed = _settingsProvider.GetSetting(Settings.HttpAgreementClosed);
            return new UIToolView(
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
                                    httpAgreementClosed
                                        ? null!
                                        : InfoBar()
                                            .Informational()
                                            .ShowIcon()
                                            .Closable()
                                            .Description(R.HttpAgreementInfoBarDescription)
                                            .OnClose(OnHttpAgreementInfoBarClose)
                                            .Open(),
                                    _packageNameInput
                                        .Title(R.PackageNameInputTitle)
                                        .CommandBarExtraContent(
                                            Wrap()
                                                .LargeSpacing()
                                                .WithChildren(
                                                    Switch()
                                                        .Off()
                                                        .OnText(R.IncludePreReleaseTitle)
                                                        .OffText(R.ExcludePreReleaseTitle)
                                                        .OnToggle(OnPreReleaseToggleChanged),
                                                    Button()
                                                        .AccentAppearance()
                                                        .Text(R.PackageLoadButtonText)
                                                        .OnClick(OnLoadPackageButtonClick)
                                                )
                                        ),
                                    _packageNameWarningBar.Warning().ShowIcon().NonClosable(),
                                    _versionRangeInput
                                        .Title(R.VersionRangeInputTitle)
                                        .OnTextChanged(OnVersionRangeInputChange),
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
        }
    }

    public ValueTask OnHttpAgreementInfoBarClose()
    {
        _settingsProvider.SetSetting(Settings.HttpAgreementClosed, true);
        return ValueTask.CompletedTask;
    }

    private async ValueTask OnLoadPackageButtonClick()
    {
        _packageNameWarningBar.Close();
        _progressRing.StartIndeterminateProgress().Show();
        _versionsList.WithChildren();

        if (string.IsNullOrWhiteSpace(_packageNameInput.Text))
        {
            _packageNameWarningBar.Description(R.PackageNameRequiredError).Open();
            _progressRing.StopIndeterminateProgress().Hide();
            return;
        }

        var package = await FetchPackage(_packageNameInput.Text);
        if (package == null)
        {
            // TODO: distinct between network error and package not found.
            _packageNameWarningBar.Description(R.PackageFetchFailureError).Open();
            _progressRing.StopIndeterminateProgress().Hide();
            return;
        }

        _versions = package.Versions;

        await OnVersionRangeInputChange(_versionRangeInput.Text);

        var list = MatchVersions();

        _versionsList.WithChildren([.. list]);
        _progressRing.StopIndeterminateProgress().Hide();
    }

    private void OnPreReleaseToggleChanged(bool isOn)
    {
        _includePreReleases = isOn;

        var list = MatchVersions();
        _versionsList.WithChildren([.. list]);
    }

    private ValueTask OnVersionRangeInputChange(string value)
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
            _versionRangeWarningBar.Description(R.VersionRangeInvalidError).Open();
            _range = null;
        }

        return ValueTask.CompletedTask;
    }

    private List<IUIElement> MatchVersions()
    {
        if (_versions == null || _versions.Count == 0)
        {
            return [];
        }

        var list = new List<IUIElement>();

        var versions = _versions.Select(v => SemVersion.Parse(v, SemVersionStyles.Strict)).ToList();
        versions.Sort(SemVersion.SortOrderComparer);

        foreach (var version in versions)
        {
            if (_includePreReleases == false && version.IsPrerelease)
            {
                continue;
            }

            var match = _range != null && _range.Contains(version);
            var text = $"{(match ? "✅" : "🔳")} {version}";
            var element = Button()
                .Text(text)
                .OnClick(() =>
                {
                    _clipboard.SetClipboardTextAsync(version.ToString()).Forget();
                });
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
            client.DefaultRequestHeaders.Add("User-Agent", "Jvw.DevToys.SemverCalculator");

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
