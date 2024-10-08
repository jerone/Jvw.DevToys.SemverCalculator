using System.ComponentModel;
using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Detectors;
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
        var fixture = new GuiFixture().WithSettingsProviderGetSettings(
            Settings.HttpAgreementClosed,
            false
        );

        // Act.
        var sut = fixture.CreateSut();

        // Assert.
        await Verify(sut.View);
    }

    [Fact]
    [Description("Snapshot GUI view with HttpAgreement info-bar not closed.")]
    public async Task Gui_View_WithHttpAgreementClosed_Snapshot()
    {
        // Arrange.
        var fixture = new GuiFixture().WithSettingsProviderGetSettings(
            Settings.HttpAgreementClosed,
            true
        );

        // Act.
        var sut = fixture.CreateSut();

        // Assert.
        await Verify(sut.View);
    }

    [Fact]
    [Description("Verify that the HTTP agreement info-bar can be closed.")]
    public async Task Gui_OnHttpAgreementInfoBarClose_ClosesInfoBar()
    {
        // Arrange.
        var fixture = new GuiFixture()
            .WithSettingsProviderGetSettings(Settings.HttpAgreementClosed, false)
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
    public void Gui_WhenPreviouslyClosesHttpAgreementInfoBar_HasClosedInfoBar()
    {
        // Arrange.
        var fixture = new GuiFixture().WithSettingsProviderGetSettings(
            Settings.HttpAgreementClosed,
            true
        );

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
        var fixture = new GuiFixture().WithSettingsProviderGetSettings(
            Settings.HttpAgreementClosed,
            false
        );
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
        const string packageName = "test-package";

        var fixture = new GuiFixture()
            .WithSettingsProviderGetSettings(Settings.HttpAgreementClosed, false)
            .WithNpmServiceFetchPackage(packageName, null);
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
    [Description("Verify that the load package button click fetches package and updates versions.")]
    public async Task Gui_OnPackageLoadButtonClick_Success_FetchesPackageAndShowsVersions()
    {
        // Arrange.
        const string packageName = "test-package";
        const string versionRange = "2.1";
        const string versionRangeInput = $"  {versionRange}  "; // Add extra whitespace that should be trimmed.
        var packageVersions = new List<string> { "1.0.0", "2.0.0", "3.0.0" };
        var package = new PackageJson { Name = packageName, Versions = packageVersions };

        var fixture = new GuiFixture()
            .WithSettingsProviderGetSettings(Settings.HttpAgreementClosed, false)
            .WithNpmServiceFetchPackage(packageName, package)
            .WithVersionServiceSetVersions(packageVersions)
            .WithVersionServiceTryParseRange(versionRange, true)
            .WithVersionServiceMatchVersions(false, [], Times.Exactly(2));
        fixture.CreateSut();

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
    }

    [Fact]
    [Description("Verify that the load package button click handles no version range.")]
    public void Gui_OnVersionRangeInputChange_MissingRange_DoesNothing()
    {
        // Arrange.
        var fixture = new GuiFixture().WithSettingsProviderGetSettings(
            Settings.HttpAgreementClosed,
            false
        );
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
            .WithSettingsProviderGetSettings(Settings.HttpAgreementClosed, false)
            .WithVersionServiceTryParseRange(versionRange, false);
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
            .WithSettingsProviderGetSettings(Settings.HttpAgreementClosed, false)
            .WithVersionServiceTryParseRange(versionRange, true)
            .WithVersionServiceMatchVersions(false, []);
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
    [Description(
        "Verify that when the pre-release toggle is on, pre-releases are included in versions."
    )]
    public void Gui_OnPreReleaseToggleChanged_ToggleOn_ShowsVersionsWithPreReleases()
    {
        // Arrange.
        var fixture = new GuiFixture()
            .WithSettingsProviderGetSettings(Settings.HttpAgreementClosed, false)
            .WithVersionServiceMatchVersions(true, []);
        fixture.CreateSut();

        var preReleaseToggle = fixture.GetElementById<IUISwitch>(Ids.PreReleaseToggle);

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
            .WithSettingsProviderGetSettings(Settings.HttpAgreementClosed, false)
            .WithVersionServiceMatchVersions(true, [])
            .WithVersionServiceMatchVersions(false, []);
        fixture.CreateSut();

        var preReleaseToggle = fixture.GetElementById<IUISwitch>(Ids.PreReleaseToggle);
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
        var fixture = new GuiFixture().WithSettingsProviderGetSettings(
            Settings.HttpAgreementClosed,
            false
        );
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
        var fixture = new GuiFixture().WithSettingsProviderGetSettings(
            Settings.HttpAgreementClosed,
            false
        );
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
            .WithSettingsProviderGetSettings(Settings.HttpAgreementClosed, false)
            .WithVersionServiceTryParseRange(versionRange, true)
            .WithVersionServiceMatchVersions(false, []);

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
