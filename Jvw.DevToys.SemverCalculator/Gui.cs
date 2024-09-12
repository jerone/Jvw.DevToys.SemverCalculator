using System.ComponentModel.Composition;
using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Components;
using Jvw.DevToys.SemverCalculator.Enums;
using Jvw.DevToys.SemverCalculator.Models;
using Jvw.DevToys.SemverCalculator.Resources;
using Jvw.DevToys.SemverCalculator.Services;
using Microsoft.Extensions.Logging;
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
    private readonly ISettingsProvider _settingsProvider;
    private readonly ILogger _logger;
    private readonly NpmService _npmService;
    private readonly VersionService _versionService;

    private readonly IUISingleLineTextInput _packageNameInput = SingleLineTextInput();
    private readonly IUIInfoBar _packageNameWarningBar = InfoBar();
    private readonly IUISingleLineTextInput _versionRangeInput = SingleLineTextInput();
    private readonly IUIInfoBar _versionRangeWarningBar = InfoBar();
    private readonly IUIWrap _versionsList = Wrap();
    private readonly IUIProgressRing _progressRing = ProgressRing();

    private bool _includePreReleases;

    [ImportingConstructor]
    public Gui(IClipboard clipboard, ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;
        _logger = this.Log();
        _npmService = new NpmService(new HttpClient(), _logger);
        _versionService = new VersionService(clipboard);

#if DEBUG
        _packageNameInput.Text("api");
        _versionRangeInput.Text("2.1 || ^3.2 || ~5.0.5 || 7.*");
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
                    .Rows((GridRow.Settings, Auto), (GridRow.Results, Auto))
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
                            SplitGrid()
                                .Vertical()
                                .AlignVertically(UIVerticalAlignment.Stretch)
                                .RightPaneLength(new UIGridLength(500, UIGridUnitType.Pixel))
                                .WithLeftPaneChild(
                                    Card(
                                            Stack()
                                                .Vertical()
                                                .AlignHorizontally(UIHorizontalAlignment.Center)
                                                .WithChildren(
                                                    _progressRing,
                                                    _versionsList.LargeSpacing()
                                                )
                                        )
                                        .AlignVertically(UIVerticalAlignment.Stretch)
                                )
                                .WithRightPaneChild(
                                    DataGrid()
                                        .Title(R.CheatSheetTitle)
                                        .ForbidSelectItem()
                                        .Extendable()
                                        .WithColumns(CheatSheetComponent.Columns)
                                        .WithRows(CheatSheetComponent.Rows)
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

        var package = await _npmService.FetchPackage(_packageNameInput.Text);
        if (package == null)
        {
            // TODO: distinct between network error and package not found.
            _packageNameWarningBar.Description(R.PackageFetchFailureError).Open();
            _progressRing.StopIndeterminateProgress().Hide();
            return;
        }

        // Save versions.
        _versionService.SetVersions(package.Versions);

        // Save version range.
        await OnVersionRangeInputChange(_versionRangeInput.Text);

        // Update versions list.
        UpdateVersionsResult();
    }

    private void OnPreReleaseToggleChanged(bool isOn)
    {
        _includePreReleases = isOn;

        UpdateVersionsResult();
    }

    private ValueTask OnVersionRangeInputChange(string value)
    {
        _versionRangeWarningBar.Close();

        if (!string.IsNullOrWhiteSpace(value))
        {
            if (_versionService.TryParseRange(value.Trim()))
            {
                UpdateVersionsResult();
            }
            else
            {
                _versionRangeWarningBar.Description(R.VersionRangeInvalidError).Open();
            }
        }

        return ValueTask.CompletedTask;
    }

    private void UpdateVersionsResult()
    {
        _progressRing.StartIndeterminateProgress().Show();

        var list = _versionService.MatchVersions(_includePreReleases);
        _versionsList.WithChildren([.. list]);

        _progressRing.StopIndeterminateProgress().Hide();
    }

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        // Smart Detection not implemented.
    }
}
