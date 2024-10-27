using System.ComponentModel;
using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Detectors;
using Jvw.DevToys.SemverCalculator.Enums;
using Jvw.DevToys.SemverCalculator.Models;
using Moq;
using R = Jvw.DevToys.SemverCalculator.Resources.Resources;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Programs;

/// <summary>
/// GUI tests for the Semver Calculator.
/// </summary>
public class GuiTests
{
    [Fact]
    [Description("Snapshot GUI view with HttpAgreement info-bar NOT closed.")]
    public async Task Gui_View_WithHttpAgreementNotClosed_Snapshot()
    {
        // Arrange.
        var fixture = new GuiFixture().WithDefaultSetup();

        // Act.
        var sut = fixture.CreateSut();

        // Assert.
        await Verify(sut.View);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Snapshot GUI view with HttpAgreement info-bar not closed.")]
    public async Task Gui_View_WithHttpAgreementClosed_Snapshot()
    {
        // Arrange.
        var fixture = new GuiFixture()
            .WithDefaultSetup()
            .WithSettingsProviderGetSettings(Settings.HttpAgreementClosed, true);

        // Act.
        var sut = fixture.CreateSut();

        // Assert.
        await Verify(sut.View);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Verify that the HTTP agreement info-bar can be closed.")]
    public async Task Gui_OnHttpAgreementInfoBarClose_ClosesInfoBar()
    {
        // Arrange.
        var fixture = new GuiFixture()
            .WithDefaultSetup()
            .WithSettingsProviderSetSettings(Settings.HttpAgreementClosed, true);
        fixture.CreateSut();
        var infoBar = fixture.GetElementById<IUIInfoBar>(Ids.HttpAgreementInfoBar);

        // Act.
        await infoBar.OnCloseAction!();

        // Assert.
        fixture.VerifyAll();
    }

    [Fact]
    [Description(
        "Verify that the HTTP agreement info-bar is closed when already previously closed."
    )]
    public void Gui_WhenPreviouslyClosedHttpAgreementInfoBar_HasClosedInfoBar()
    {
        // Arrange.
        var fixture = new GuiFixture()
            .WithDefaultSetup()
            .WithSettingsProviderGetSettings(Settings.HttpAgreementClosed, true);

        // Act.
        fixture.CreateSut();

        // Assert.
        Assert.Null(fixture.GetElementById(Ids.HttpAgreementInfoBar));
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Verify that the load package button click handles empty package name.")]
    public async Task Gui_OnPackageLoadButtonClick_EmptyPackageName_ShowsWarning()
    {
        // Arrange.
        var fixture = new GuiFixture().WithDefaultSetup();
        fixture.CreateSut();

        fixture.GetElementById<IUISingleLineTextInput>(Ids.PackageNameInput).Text(string.Empty);
        var packageLoadButton = fixture.GetElementById<IUIButton>(Ids.PackageLoadButton);
        var packageNameWarningBar = fixture.GetElementById<IUIInfoBar>(Ids.PackageNameWarningBar);

        // Act.
        await packageLoadButton.OnClickAction!();

        // Assert.
        Assert.True(packageNameWarningBar.IsOpened);
        Assert.Equal(R.PackageNameRequiredError, packageNameWarningBar.Description);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Verify that the load package button click handles package fetch failure.")]
    public async Task Gui_OnPackageLoadButtonClick_PackageFetchFailure_ShowsWarning()
    {
        // Arrange.
        const string packageName = "  test-package  "; // Whitespaces should be trimmed.

        var fixture = new GuiFixture()
            .WithDefaultSetup()
            .WithPackageManagerServiceFetchPackage(packageName.Trim(), null);
        fixture.CreateSut();

        fixture.GetElementById<IUISingleLineTextInput>(Ids.PackageNameInput).Text(packageName);
        var packageLoadButton = fixture.GetElementById<IUIButton>(Ids.PackageLoadButton);
        var packageNameWarningBar = fixture.GetElementById<IUIInfoBar>(Ids.PackageNameWarningBar);

        // Act.
        await packageLoadButton.OnClickAction!();

        // Assert.
        Assert.True(packageNameWarningBar.IsOpened);
        Assert.Equal(R.PackageFetchFailureError, packageNameWarningBar.Description);
        fixture.VerifyAll();
    }

    [Fact]
    [Description(
        "Verify that the load package button click fetches package and creates a list of versions."
    )]
    public async Task Gui_OnPackageLoadButtonClick_Success_FetchesPackageAndShowsVersions()
    {
        // Arrange.
        const string packageName = "test-package";
        const string versionRange = "2.1";
        const string versionRangeInput = $"  {versionRange}  "; // Add extra whitespace that should be trimmed.
        var packageVersions = new List<string> { "1.0.0", "2.0.0", "3.0.0" };
        IEnumerable<(string version, bool match)> versionsResult =
        [
            ("1.0.0", false),
            ("2.0.0", false),
            ("3.0.0", true), // Version that matches.
        ];

        var fixture = new GuiFixture()
            .WithDefaultSetup()
            .WithPackageManagerServiceFetchPackage(packageName, packageVersions)
            .WithPackageManagerServiceSetVersions(packageVersions)
            .WithPackageManagerServiceTryParseRange(versionRange, true)
            .WithPackageManagerServiceGetVersions(false, versionsResult, Times.Exactly(3));
        _ = fixture.CreateSut();

        fixture.GetElementById<IUISingleLineTextInput>(Ids.PackageNameInput).Text(packageName);
        fixture
            .GetElementById<IUISingleLineTextInput>(Ids.VersionRangeInput)
            .OnTextChanged(null!) // We don't want the input to trigger change event.
            .Text(versionRangeInput);
        var packageLoadButton = fixture.GetElementById<IUIButton>(Ids.PackageLoadButton);

        // Act.
        await packageLoadButton.OnClickAction!();

        // Assert.
        fixture.VerifyAll();
        await Verify(fixture.GetElementById<IUIWrap>(Ids.VersionsList));
    }

    [Fact]
    [Description(
        "Verify that clicking on UI version button triggers the clipboard with correct version."
    )]
    public async Task Gui_OnVersionButtonClick_Success_TriggersClipboardWithVersion()
    {
        // Arrange.
        const string packageName = "test-package";
        var packageVersions = new List<string> { "1.0.0" };
        IEnumerable<(string version, bool match)> versionsResult = [("1.0.0", false)];

        var fixture = new GuiFixture()
            .WithDefaultSetup()
            .WithPackageManagerServiceFetchPackage(packageName, packageVersions)
            .WithPackageManagerServiceSetVersions(packageVersions)
#if DEBUG
            .WithPackageManagerServiceTryParseRange("2.1 || ^3.2 || ~5.0.5 || 7.*", true)
            .WithPackageManagerServiceGetVersions(false, versionsResult, Times.Exactly(3))
#else
            .WithPackageManagerServiceGetVersions(false, versionsResult, Times.Exactly(2))
#endif
            .WithClipboardSetClipboardTextAsync("1.0.0");
        _ = fixture.CreateSut();

        fixture.GetElementById<IUISingleLineTextInput>(Ids.PackageNameInput).Text(packageName);
        var packageLoadButton = fixture.GetElementById<IUIButton>(Ids.PackageLoadButton);

        // Act.
        await packageLoadButton.OnClickAction!();
        await (
            (IUIButton)fixture.GetElementById<IUIWrap>(Ids.VersionsList).Children![0]
        ).OnClickAction!();

        // Assert.
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Verify that the load package button click handles no version range.")]
    public void Gui_OnVersionRangeInputChange_MissingRange_DoesNothing()
    {
        // Arrange.
        var fixture = new GuiFixture().WithDefaultSetup();
        fixture.CreateSut();

        var versionRangeInput = fixture.GetElementById<IUISingleLineTextInput>(
            Ids.VersionRangeInput
        );
        var versionRangeWarningBar = fixture.GetElementById<IUIInfoBar>(Ids.VersionRangeWarningBar);

        // Act.
        versionRangeInput.Text(string.Empty);

        // Assert.
        Assert.False(versionRangeWarningBar.IsOpened);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Verify that the load package button click handles invalid version range.")]
    public void Gui_OnVersionRangeInputChange_InvalidRange_ShowsWarning()
    {
        // Arrange.
        const string versionRange = "invalid version range";

        var fixture = new GuiFixture()
            .WithDefaultSetup()
            .WithPackageManagerServiceTryParseRange(versionRange, false);
        fixture.CreateSut();

        var versionRangeInput = fixture.GetElementById<IUISingleLineTextInput>(
            Ids.VersionRangeInput
        );
        var versionRangeWarningBar = fixture.GetElementById<IUIInfoBar>(Ids.VersionRangeWarningBar);

        // Act.
        versionRangeInput.Text(versionRange);

        // Assert.
        Assert.True(versionRangeWarningBar.IsOpened);
        Assert.Equal(R.VersionRangeInvalidError, versionRangeWarningBar.Description);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Verify that the load package button click handles invalid version range.")]
    public void Gui_OnVersionRangeInputChange_ValidRange_ShowsVersions()
    {
        // Arrange.
        const string versionRange = "1.2.3";

        var fixture = new GuiFixture()
            .WithDefaultSetup()
            .WithPackageManagerServiceTryParseRange(versionRange, true)
            .WithPackageManagerServiceGetVersions(false, [], Times.Exactly(2));
        fixture.CreateSut();

        var versionRangeInput = fixture.GetElementById<IUISingleLineTextInput>(
            Ids.VersionRangeInput
        );
        var versionRangeWarningBar = fixture.GetElementById<IUIInfoBar>(Ids.VersionRangeWarningBar);

        // Act.
        versionRangeInput.Text(versionRange);

        // Assert.
        Assert.False(versionRangeWarningBar.IsOpened);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Setting NPM as package manager, shows NPM cheat sheet.")]
    public void Gui_OnPackageManagerSettingChanged_SetNpm_ShowsNpmCheatSheet()
    {
        // Arrange.
        var fixture = new GuiFixture()
            .WithPackageManagerFactoryLoad(PackageManager.Npm)
            .WithPackageManagerFactoryLoad(PackageManager.NuGet)
            .WithPackageManagerServiceSetVersions([], Times.Exactly(2))
            .WithSettingsProviderGetSettings(Settings.HttpAgreementClosed)
            .WithSettingsProviderGetSettings(Settings.IncludePreReleases)
            .WithSettingsProviderGetSettings(
                Settings.PackageManager,
                PackageManager.NuGet,
                Times.Exactly(2)
            )
            .WithSettingsProviderSettingChanged()
            .WithSettingsProviderSetSettings(Settings.PackageManager, PackageManager.Npm)
#if DEBUG
            .WithPackageManagerServiceTryParseRange("2.1 || ^3.2 || ~5.0.5 || 7.*", true)
            .WithPackageManagerServiceGetVersions(false, [], Times.Exactly(3))
#else
            .WithPackageManagerServiceGetVersions(false, [], Times.Exactly(2))
#endif
        ;
        _ = fixture.CreateSut();

        var packageManagerSetting = fixture.GetElementById<IUISetting>(Ids.PackageManagerSetting);
        var packageManagerDropDown = Assert.IsAssignableFrom<IUISelectDropDownList>(
            packageManagerSetting.InteractiveElement
        );
        // As the default is NPM, we need to active NuGet first, to be able to switch to NPM and trigger change event.
        packageManagerDropDown.Select(
            packageManagerDropDown.Items!.First(x => x.Value!.Equals(PackageManager.NuGet))
        );

        // Act.
        packageManagerDropDown.Select(
            packageManagerDropDown.Items!.First(x => x.Value!.Equals(PackageManager.Npm))
        );

        // Assert.
        Assert.Equal(PackageManager.Npm, packageManagerDropDown.SelectedItem!.Value);
        Assert.True(fixture.GetElementById<IUIDataGrid>(Ids.CheatSheetNpmDataGrid).IsVisible);
        Assert.False(fixture.GetElementById<IUIDataGrid>(Ids.CheatSheetNuGetDataGrid).IsVisible);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Setting NuGet as package manager, shows NuGet cheat sheet.")]
    public void Gui_OnPackageManagerSettingChanged_SetNuGet_ShowsNpmCheatSheet()
    {
        // Arrange.
        var fixture = new GuiFixture()
            .WithPackageManagerFactoryLoad(PackageManager.Npm)
            .WithPackageManagerFactoryLoad(PackageManager.NuGet)
            .WithPackageManagerServiceSetVersions([], Times.Exactly(2))
            .WithSettingsProviderGetSettings(Settings.HttpAgreementClosed)
            .WithSettingsProviderGetSettings(Settings.IncludePreReleases)
            .WithSettingsProviderGetSettings(
                Settings.PackageManager,
                PackageManager.Npm,
                Times.Exactly(2)
            )
            .WithSettingsProviderSettingChanged()
            .WithSettingsProviderSetSettings(Settings.PackageManager, PackageManager.NuGet)
#if DEBUG
            .WithPackageManagerServiceTryParseRange("2.1 || ^3.2 || ~5.0.5 || 7.*", true)
            .WithPackageManagerServiceGetVersions(false, [], Times.Exactly(3))
#else
            .WithPackageManagerServiceGetVersions(false, [], Times.Exactly(2))
#endif
        ;
        _ = fixture.CreateSut();

        var packageManagerSetting = fixture.GetElementById<IUISetting>(Ids.PackageManagerSetting);
        var packageManagerDropDown = Assert.IsAssignableFrom<IUISelectDropDownList>(
            packageManagerSetting.InteractiveElement
        );

        // Act.
        packageManagerDropDown.Select(
            packageManagerDropDown.Items!.First(x => x.Value!.Equals(PackageManager.NuGet))
        );

        // Assert.
        Assert.Equal(PackageManager.NuGet, packageManagerDropDown.SelectedItem!.Value);
        Assert.False(fixture.GetElementById<IUIDataGrid>(Ids.CheatSheetNpmDataGrid).IsVisible);
        Assert.True(fixture.GetElementById<IUIDataGrid>(Ids.CheatSheetNuGetDataGrid).IsVisible);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Setting unknown package manager, shows no cheat sheet.")]
    public void Gui_OnPackageManagerSettingChanged_SetUnknownPackageManager_ShowsNoCheatSheet()
    {
        // Arrange.
        var fixture = new GuiFixture()
            .WithPackageManagerFactoryLoad(PackageManager.Npm)
            .WithPackageManagerFactoryLoad((PackageManager)256)
            .WithPackageManagerServiceSetVersions([], Times.Exactly(2))
            .WithSettingsProviderGetSettings(Settings.HttpAgreementClosed)
            .WithSettingsProviderGetSettings(Settings.IncludePreReleases)
            .WithSettingsProviderGetSettings(
                Settings.PackageManager,
                PackageManager.Npm,
                Times.Exactly(2)
            )
            .WithSettingsProviderSettingChanged()
            .WithSettingsProviderSetSettings(Settings.PackageManager, (PackageManager)256)
#if DEBUG
            .WithPackageManagerServiceTryParseRange("2.1 || ^3.2 || ~5.0.5 || 7.*", true)
            .WithPackageManagerServiceGetVersions(false, [], Times.Exactly(3))
#else
            .WithPackageManagerServiceGetVersions(false, [], Times.Exactly(2))
#endif
        ;
        _ = fixture.CreateSut();

        var packageManagerSetting = fixture.GetElementById<IUISetting>(Ids.PackageManagerSetting);
        var packageManagerDropDown = Assert.IsAssignableFrom<IUISelectDropDownList>(
            packageManagerSetting.InteractiveElement
        );
        // As the default is NuGet, we need to active NPM first, to be able to switch to NuGet and trigger change event.
        var dropDownListItemMock = new Mock<IUIDropDownListItem>(MockBehavior.Strict);
        dropDownListItemMock.SetupGet(x => x.Value).Returns((PackageManager)256);
        packageManagerDropDown.WithItems(
            [.. packageManagerDropDown.Items!, dropDownListItemMock.Object]
        );

        // Act.
        packageManagerDropDown.Select(
            packageManagerDropDown.Items!.First(x => x.Value!.Equals((PackageManager)256))
        );

        // Assert.
        Assert.Equal((PackageManager)256, packageManagerDropDown.SelectedItem!.Value);
        Assert.False(fixture.GetElementById<IUIDataGrid>(Ids.CheatSheetNpmDataGrid).IsVisible);
        Assert.False(fixture.GetElementById<IUIDataGrid>(Ids.CheatSheetNuGetDataGrid).IsVisible);
        fixture.VerifyAll();
        dropDownListItemMock.VerifyAll();
    }

    [Fact]
    [Description(
        "Verify that when the pre-release toggle is on, pre-releases are included in versions."
    )]
    public void Gui_OnPreReleaseToggleChanged_ToggleOn_ShowsVersionsWithPreReleases()
    {
        // Arrange.
        var fixture = new GuiFixture()
            .WithDefaultSetup()
            .WithSettingsProviderSetSettings(Settings.IncludePreReleases, true)
            .WithPackageManagerServiceGetVersions(true, []);
        fixture.CreateSut();

        var preReleaseSetting = fixture.GetElementById<IUISetting>(Ids.PreReleaseToggle);
        var preReleaseToggle = Assert.IsAssignableFrom<IUISwitch>(
            preReleaseSetting.InteractiveElement
        );

        // Act.
        preReleaseToggle.On();

        // Assert.
        Assert.True(preReleaseToggle.IsOn);
        fixture.VerifyAll();
    }

    [Fact]
    [Description(
        "Verify that when the pre-release toggle is off, pre-releases are excluded in versions."
    )]
    public void Gui_OnPreReleaseToggleChanged_ToggleOff_ShowsVersionsWithoutPreReleases()
    {
        // Arrange.
        var fixture = new GuiFixture()
            .WithDefaultSetup()
            .WithSettingsProviderSetSettings(Settings.IncludePreReleases, true)
            .WithSettingsProviderSetSettings(Settings.IncludePreReleases, false)
            .WithPackageManagerServiceGetVersions(true, [])
            .WithPackageManagerServiceGetVersions(false, [], Times.Exactly(2));
        fixture.CreateSut();

        var preReleaseSetting = fixture.GetElementById<IUISetting>(Ids.PreReleaseToggle);
        var preReleaseToggle = Assert.IsAssignableFrom<IUISwitch>(
            preReleaseSetting.InteractiveElement
        );
        preReleaseToggle.On(); // As the default is off, we first need to turn it on, to be able to turn it off.

        // Act.
        preReleaseToggle.Off();

        // Assert.
        Assert.False(preReleaseToggle.IsOn);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Verify that version range input is not changed when no data detector.")]
    public void Gui_OnDataReceived_WhenNoDetector_DoesNotChangeVersionRange()
    {
        // Arrange.
        var fixture = new GuiFixture().WithDefaultSetup();
        var sut = fixture.CreateSut();

        var versionRangeInput = fixture.GetElementById<IUISingleLineTextInput>(
            Ids.VersionRangeInput
        );
        var defaultVersionRangeInputValue = versionRangeInput.Text;

        // Act.
        sut.OnDataReceived(null, null);

        // Assert.
        Assert.Equal(defaultVersionRangeInputValue, versionRangeInput.Text);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Verify that version range input is not changed when detector has no data.")]
    public void Gui_OnDataReceived_WhenSemVerRangeDetectorButNoData_DoesNotChangeVersionRange()
    {
        // Arrange.
        var fixture = new GuiFixture().WithDefaultSetup();
        var sut = fixture.CreateSut();

        var versionRangeInput = fixture.GetElementById<IUISingleLineTextInput>(
            Ids.VersionRangeInput
        );
        var defaultVersionRangeInputValue = versionRangeInput.Text;

        // Act.
        sut.OnDataReceived(SemVersionRangeDataTypeDetector.Name, null);

        // Assert.
        Assert.Equal(defaultVersionRangeInputValue, versionRangeInput.Text);
        fixture.VerifyAll();
    }

    [Fact]
    [Description("Verify that version range input is updated when SemVer range detector has data.")]
    public void Gui_OnDataReceived_WhenSemVerRangeDetectorWithData_ChangesVersionRange()
    {
        // Arrange.
        const string versionRange = "1.2.3";

        var fixture = new GuiFixture()
            .WithDefaultSetup()
            .WithPackageManagerServiceTryParseRange(versionRange, true)
            .WithPackageManagerServiceGetVersions(false, [], Times.Exactly(2));
        var sut = fixture.CreateSut();

        var versionRangeInput = fixture.GetElementById<IUISingleLineTextInput>(
            Ids.VersionRangeInput
        );

        // Act.
        sut.OnDataReceived(SemVersionRangeDataTypeDetector.Name, versionRange);

        // Assert.
        Assert.Equal(versionRange, versionRangeInput.Text);
        fixture.VerifyAll();
    }
}
