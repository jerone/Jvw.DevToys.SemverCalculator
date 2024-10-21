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
    IconFontName = Icons.FontName,
    IconGlyph = Icons.Calculator,
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
    AccessibleNameResourceName = nameof(R.AccessibleNameResourceName),
    SearchKeywordsResourceName = nameof(R.SearchKeywordsResourceName)
)]
[AcceptedDataTypeName(SemVersionRangeDataTypeDetector.Name)]
internal sealed class Gui : IGuiTool
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly IClipboard _clipboard;
    private readonly IPackageManagerFactory _packageManagerFactory;

    private IPackageManagerService PackageManagerService { get; set; } = null!;

    private readonly IUISingleLineTextInput _packageNameInput = SingleLineTextInput(
        Ids.PackageNameInput
    );
    private readonly IUIInfoBar _packageNameWarningBar = InfoBar(Ids.PackageNameWarningBar);
    private readonly IUISingleLineTextInput _versionRangeInput = SingleLineTextInput(
        Ids.VersionRangeInput
    );
    private readonly IUIInfoBar _versionRangeWarningBar = InfoBar(Ids.VersionRangeWarningBar);
    private readonly IUIProgressRing _progressRing = ProgressRing(Ids.ProgressRing);

    // ReSharper disable InconsistentNaming -- Internal until https://github.com/DevToys-app/DevToys/issues/1406 is fixed.
    internal readonly IUIWrap _versionsList = Wrap(Ids.VersionsList);
    internal readonly IUIDataGrid _cheatSheetNpmDataGrid = CheatSheetComponent.CheatSheetNpm();
    internal readonly IUIDataGrid _cheatSheetNuGetDataGrid = CheatSheetComponent.CheatSheetNuGet();

    // ReSharper restore InconsistentNaming

    private bool _includePreReleases;

    [ImportingConstructor]
    public Gui(
        ISettingsProvider settingsProvider,
        IClipboard clipboard,
        IPackageManagerFactory packageManagerFactory
    )
    {
        _settingsProvider = settingsProvider;
        _clipboard = clipboard;
        _packageManagerFactory = packageManagerFactory;

        var packageManager = _settingsProvider.GetSetting(Settings.PackageManager);
        _ = OnPackageManagerSettingChanged(packageManager);

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
                                    Label().Text(R.SettingsTitle),
                                    Setting(Ids.PackageManagerSetting)
                                        .Title(R.SettingPackageManagerTitle)
                                        .Description(R.SettingPackageManagerDescription)
                                        .Icon(Icons.FontName, Icons.Box)
                                        .Handle(
                                            _settingsProvider,
                                            Settings.PackageManager,
                                            OnPackageManagerSettingChanged,
                                            Item(R.PackageManagerNpmName, PackageManager.Npm),
                                            Item(R.PackageManagerNuGetName, PackageManager.NuGet)
                                        ),
                                    Setting(Ids.PreReleaseToggle)
                                        .Title(R.SettingPreReleaseTitle)
                                        .Icon(Icons.FontName, Icons.Beaker)
                                        .Handle(
                                            _settingsProvider,
                                            Settings.IncludePreReleases,
                                            OnPreReleaseSettingChanged
                                        ),
                                    _packageNameInput
                                        .Title(R.PackageNameInputTitle)
                                        .CommandBarExtraContent(
                                            // TODO: move button down.
                                            Button(Ids.PackageLoadButton)
                                                .AccentAppearance()
                                                .Text(R.PackageLoadButtonText)
                                                .OnClick(OnPackageLoadButtonClick)
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
                                    Stack()
                                        .Vertical()
                                        .AlignVertically(UIVerticalAlignment.Top)
                                        .WithChildren(
                                            _cheatSheetNpmDataGrid,
                                            _cheatSheetNuGetDataGrid
                                        )
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
    /// Event triggered when package manager setting is changed.
    /// </summary>
    /// <param name="packageManager">Package manager.</param>
    /// <returns>Task.</returns>
    private async ValueTask OnPackageManagerSettingChanged(PackageManager packageManager)
    {
        PackageManagerService = _packageManagerFactory.Load(packageManager);

        // Clear versions.
        PackageManagerService.SetVersions([]);

        // Validate and save version range.
        await OnVersionRangeInputChange(_versionRangeInput.Text);

        // Update versions list.
        UpdateVersionsResult();

        // Update cheat sheet.
        switch (packageManager)
        {
            default:
            case PackageManager.Npm:
                _cheatSheetNpmDataGrid.Show();
                _cheatSheetNuGetDataGrid.Hide();
                break;
            case PackageManager.NuGet:
                _cheatSheetNpmDataGrid.Hide();
                _cheatSheetNuGetDataGrid.Show();
                break;
        }
    }

    /// <summary>
    /// Event triggered when include pre-releases toggle changes.
    /// </summary>
    /// <param name="isOn">Toggle is on or off.</param>
    private void OnPreReleaseSettingChanged(bool isOn)
    {
        _includePreReleases = isOn;

        UpdateVersionsResult();
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

        var packageName = _packageNameInput.Text.Trim();
        if (packageName == string.Empty)
        {
            _packageNameWarningBar.Description(R.PackageNameRequiredError).Open();
            _progressRing.StopIndeterminateProgress().Hide();
            return;
        }

        var versions = await PackageManagerService.FetchPackage(packageName);
        if (versions == null)
        {
            // TODO: distinct between network error and package not found.
            _packageNameWarningBar.Description(R.PackageFetchFailureError).Open();
            _progressRing.StopIndeterminateProgress().Hide();
            return;
        }

        // Save versions.
        PackageManagerService.SetVersions(versions);

        // Validate and save version range.
        await OnVersionRangeInputChange(_versionRangeInput.Text);

        // Update versions list.
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

        if (!PackageManagerService.TryParseRange(value.Trim()))
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

        var list = new List<IUIElement>();

        var versions = PackageManagerService.GetVersions(_includePreReleases);
        foreach (var (version, match) in versions)
        {
            var element = Button()
                .Icon(Icons.FontName, match ? Icons.Checkbox : Icons.CheckboxUnchecked)
                .Text(version)
                .OnClick(OnVersionButtonClick(version));
            if (match)
            {
                element.AccentAppearance();
            }
            list.Add(element);
        }

        _versionsList.WithChildren([.. list]);

        _progressRing.StopIndeterminateProgress().Hide();
    }

    /// <summary>
    /// Event triggered when version button is clicked.
    /// </summary>
    /// <param name="version">SemVer version.</param>
    /// <returns>Event.</returns>
    private Action OnVersionButtonClick(string version)
    {
        return () =>
        {
            _clipboard.SetClipboardTextAsync(version).Forget();
        };
    }

    /// <inheritdoc cref="IGuiTool.OnDataReceived" />
    public void OnDataReceived(string? dataTypeName, object? parsedData)
    {
        // Set version range input, when valid semver range is detected and received.
        if (dataTypeName == SemVersionRangeDataTypeDetector.Name && parsedData is string dataString)
        {
            _versionRangeInput.Text(dataString);
        }
    }
}
