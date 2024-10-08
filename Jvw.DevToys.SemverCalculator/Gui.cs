using System.ComponentModel.Composition;
using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Components;
using Jvw.DevToys.SemverCalculator.Detectors;
using Jvw.DevToys.SemverCalculator.Enums;
using Jvw.DevToys.SemverCalculator.Models;
using Jvw.DevToys.SemverCalculator.Resources;
using Jvw.DevToys.SemverCalculator.Services;
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
[AcceptedDataTypeName(SemVersionRangeDataTypeDetector.Name)]
internal sealed class Gui : IGuiTool
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly INpmService _npmService;
    private readonly IVersionService _versionService;

    private readonly IUISingleLineTextInput _packageNameInput = SingleLineTextInput(
        Ids.PackageNameInput
    );
    private readonly IUIInfoBar _packageNameWarningBar = InfoBar(Ids.PackageNameWarningBar);
    private readonly IUISingleLineTextInput _versionRangeInput = SingleLineTextInput(
        Ids.VersionRangeInput
    );
    private readonly IUIInfoBar _versionRangeWarningBar = InfoBar(Ids.VersionRangeWarningBar);
    private readonly IUIWrap _versionsList = Wrap(Ids.VersionsList);
    private readonly IUIProgressRing _progressRing = ProgressRing(Ids.ProgressRing);

    private bool _includePreReleases;

    [ImportingConstructor]
    public Gui(
        ISettingsProvider settingsProvider,
        INpmService npmService,
        IVersionService versionService
    )
    {
        _settingsProvider = settingsProvider;
        _npmService = npmService;
        _versionService = versionService;

#if DEBUG
        _packageNameInput.Text("api");
        _versionRangeInput.Text("2.1 || ^3.2 || ~5.0.5 || 7.*");
#endif
    }

    private UIToolView? _view;
    public UIToolView View
    {
        get
        {
            if (_view != null)
                return _view;

            var httpAgreementClosed = _settingsProvider.GetSetting(Settings.HttpAgreementClosed);
            return _view = new UIToolView(
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
                                        : InfoBar(Ids.HttpAgreementInfoBar)
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
                                                    Switch(Ids.PreReleaseToggle)
                                                        .Off()
                                                        .OnText(R.IncludePreReleaseTitle)
                                                        .OffText(R.ExcludePreReleaseTitle)
                                                        .OnToggle(OnPreReleaseToggleChanged),
                                                    Button(Ids.PackageLoadButton)
                                                        .AccentAppearance()
                                                        .Text(R.PackageLoadButtonText)
                                                        .OnClick(OnPackageLoadButtonClick)
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

    /// <summary>
    /// Event triggered when the HTTP agreement info bar is closed.
    /// </summary>
    /// <returns>Task.</returns>
    private ValueTask OnHttpAgreementInfoBarClose()
    {
        _settingsProvider.SetSetting(Settings.HttpAgreementClosed, true);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Event triggered when load package button is clicked.
    /// </summary>
    /// <returns>Task.</returns>
    private async ValueTask OnPackageLoadButtonClick()
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

    /// <summary>
    /// Event triggered when include pre-releases toggle changes.
    /// </summary>
    /// <param name="isOn">Toggle is on or off.</param>
    private void OnPreReleaseToggleChanged(bool isOn)
    {
        _includePreReleases = isOn;

        UpdateVersionsResult();
    }

    /// <summary>
    /// Event triggered when input for version range changes.
    /// </summary>
    /// <param name="value">Version range.</param>
    /// <returns>Task.</returns>
    private ValueTask OnVersionRangeInputChange(string value)
    {
        _versionRangeWarningBar.Close();

        if (string.IsNullOrWhiteSpace(value))
        {
            return ValueTask.CompletedTask;
        }

        if (!_versionService.TryParseRange(value.Trim()))
        {
            _versionRangeWarningBar.Description(R.VersionRangeInvalidError).Open();
            return ValueTask.CompletedTask;
        }

        UpdateVersionsResult();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Update the versions result list.
    /// </summary>
    private void UpdateVersionsResult()
    {
        _progressRing.StartIndeterminateProgress().Show();

        var list = _versionService.MatchVersions(_includePreReleases);
        _versionsList.WithChildren([.. list]);

        _progressRing.StopIndeterminateProgress().Hide();
    }

    /// <inheritdoc cref="IGuiTool.OnDataReceived" />
    public void OnDataReceived(string? dataTypeName, object? parsedData)
    {
        // Set version range input, when semver range is detected and received.
        if (dataTypeName == SemVersionRangeDataTypeDetector.Name && parsedData is string dataString)
        {
            _versionRangeInput.Text(dataString);
        }
    }
}
