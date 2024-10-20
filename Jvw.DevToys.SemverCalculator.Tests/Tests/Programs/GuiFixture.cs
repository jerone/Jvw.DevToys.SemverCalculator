using System.Diagnostics.CodeAnalysis;
using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Enums;
using Jvw.DevToys.SemverCalculator.Models;
using Jvw.DevToys.SemverCalculator.Services;
using Moq;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Programs;

/// <summary>
/// Fixture for GUI tests.
/// </summary>
[SuppressMessage(
    "PosInformatique.Moq.Analyzers",
    "PosInfoMoq1002",
    Justification = "Verification is handled in VerifyAll method."
)]
internal class GuiFixture : IBaseFixture<Gui, GuiFixture>
{
    private readonly Mock<ISettingsProvider> _settingsProviderMock = new(MockBehavior.Strict);
    private readonly Mock<IClipboard> _clipboardMock = new(MockBehavior.Strict);
    private readonly Mock<IPackageManagerFactory> _packageManagerFactoryMock =
        new(MockBehavior.Strict);
    private readonly Mock<IPackageManagerService> _packageManagerServiceMock =
        new(MockBehavior.Strict);

    private Gui Sut { get; set; } = null!;

    /// <inheritdoc cref="IBaseFixture{TSut,TFixture}.CreateSut" />
    public Gui CreateSut()
    {
        Sut = new Gui(
            _settingsProviderMock.Object,
            _clipboardMock.Object,
            _packageManagerFactoryMock.Object
        );

        return Sut;
    }

    /// <summary>
    /// Get typed element by identifier.
    /// </summary>
    /// <remarks>This method includes assertion.</remarks>
    /// <typeparam name="TElement">Type of element.</typeparam>
    /// <param name="id">Identifier.</param>
    /// <returns>Typed element.</returns>
    internal TElement GetElementById<TElement>(string id)
    {
        var element = Sut.View.GetChildElementById(id);
        Assert.NotNull(element);
        Assert.IsAssignableFrom<TElement>(element);
        return (TElement)element;
    }

    /// <summary>
    /// Get element by identifier.
    /// </summary>
    /// <remarks>This method does no assertion.</remarks>
    /// <param name="id">Identifier.</param>
    /// <returns>Element.</returns>
    internal IUIElement? GetElementById(string id)
    {
        var element = Sut.View.GetChildElementById(id);
        return element;
    }

    /// <inheritdoc cref="IBaseFixture{TSut,TFixture}.VerifyAll" />
    public GuiFixture VerifyAll()
    {
        _settingsProviderMock.VerifyAll();
        _settingsProviderMock.VerifyNoOtherCalls();
        _clipboardMock.VerifyAll();
        _clipboardMock.VerifyNoOtherCalls();
        _packageManagerFactoryMock.VerifyAll();
        _packageManagerFactoryMock.VerifyNoOtherCalls();
        _packageManagerServiceMock.VerifyAll();
        _packageManagerServiceMock.VerifyNoOtherCalls();
        return this;
    }

    /// <summary>
    /// Setup default mock values, that are the same for every test.
    /// </summary>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiFixture WithDefaultSetup()
    {
        WithPackageManagerFactoryLoad(PackageManager.Npm);
        WithPackageManagerServiceSetVersions([]);
        WithPackageManagerServiceGetVersions(false, []);
        WithSettingsProviderGetSettings(Settings.HttpAgreementClosed);
        WithSettingsProviderGetSettings(Settings.PackageManager, Times.Exactly(2));
        WithSettingsProviderGetSettings(Settings.IncludePreReleases);
        WithSettingsProviderSettingChanged();
        return this;
    }

    /// <summary>
    /// Setup mock for `SettingsProvider.GetSetting` with default return value.
    /// </summary>
    /// <typeparam name="T">Type of value of the setting.</typeparam>
    /// <param name="key">Setting key.</param>
    /// <param name="times">Optional verify times. Default is once.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiFixture WithSettingsProviderGetSettings<T>(
        SettingDefinition<T> key,
        Times? times = null
    )
    {
        _settingsProviderMock
            .Setup(x => x.GetSetting(key))
            .Returns(key.DefaultValue)
            .Verifiable(times ?? Times.Once());
        return this;
    }

    /// <summary>
    /// Setup mock for `SettingsProvider.GetSetting` with return value.
    /// </summary>
    /// <typeparam name="T">Type of value of the setting.</typeparam>
    /// <param name="key">Setting key.</param>
    /// <param name="value">Setting value.</param>
    /// <param name="times">Optional verify times. Default is once.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiFixture WithSettingsProviderGetSettings<T>(
        SettingDefinition<T> key,
        T value,
        Times? times = null
    )
    {
        _settingsProviderMock
            .Setup(x => x.GetSetting(key))
            .Returns(value)
            .Verifiable(times ?? Times.Once());
        return this;
    }

    /// <summary>
    /// Setup mock for `SettingsProvider.SettingChanged` with event.
    /// </summary>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiFixture WithSettingsProviderSettingChanged()
    {
        _settingsProviderMock
            .SetupAdd(m => m.SettingChanged += It.IsAny<EventHandler<SettingChangedEventArgs>?>())
            .Verifiable();
        return this;
    }

    /// <summary>
    /// Setup mock for `SettingsProvider.SetSetting`.
    /// </summary>
    /// <typeparam name="T">Type of value of the setting.</typeparam>
    /// <param name="key">Setting key.</param>
    /// <param name="value">Setting value.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiFixture WithSettingsProviderSetSettings<T>(SettingDefinition<T> key, T value)
    {
        _settingsProviderMock.Setup(x => x.SetSetting(key, value)).Verifiable(Times.Once);
        return this;
    }

    /// <summary>
    /// Setup mock for `IPackageManagerFactory.Load` with return value.
    /// </summary>
    /// <param name="packageManager">Package manager.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiFixture WithPackageManagerFactoryLoad(PackageManager packageManager)
    {
        _packageManagerFactoryMock
            .Setup(x => x.Load(packageManager))
            .Returns(_packageManagerServiceMock.Object)
            .Verifiable(Times.Once);
        return this;
    }

    /// <summary>
    /// Setup mock for `IPackageManagerService.SetVersions`.
    /// </summary>
    /// <param name="packageVersions">Package versions.</param>
    /// <param name="times">Optional verify times. Default is once.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiFixture WithPackageManagerServiceSetVersions(
        List<string> packageVersions,
        Times? times = null
    )
    {
        _packageManagerServiceMock
            .Setup(x => x.SetVersions(packageVersions))
            .Verifiable(times ?? Times.Once());
        return this;
    }

    /// <summary>
    /// Setup mock for `IPackageManagerService.TryParseRange` with return value.
    /// </summary>
    /// <param name="range">Version range.</param>
    /// <param name="result">Result of the parsing.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiFixture WithPackageManagerServiceTryParseRange(string range, bool result)
    {
        _packageManagerServiceMock
            .Setup(x => x.TryParseRange(range))
            .Returns(result)
            .Verifiable(Times.Once());
        return this;
    }

    /// <summary>
    /// Setup mock for `IPackageManagerService.GetVersions` with return value.
    /// </summary>
    /// <param name="includePreReleases">Include pre-releases.</param>
    /// <param name="result">Result of the matching.</param>
    /// <param name="times">Optional verify times. Default is once.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiFixture WithPackageManagerServiceGetVersions(
        bool includePreReleases,
        IEnumerable<(string version, bool match)> result,
        Times? times = null
    )
    {
        _packageManagerServiceMock
            .Setup(x => x.GetVersions(includePreReleases))
            .Returns(result)
            .Verifiable(times ?? Times.Once());
        return this;
    }

    /// <summary>
    /// Setup mock for `IPackageManagerService.FetchPackage` with return value.
    /// </summary>
    /// <param name="packageName">Package name.</param>
    /// <param name="packageVersions">Package versions.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiFixture WithPackageManagerServiceFetchPackage(
        string packageName,
        List<string>? packageVersions
    )
    {
        _packageManagerServiceMock
            .Setup(x => x.FetchPackage(packageName))
            .ReturnsAsync(packageVersions)
            .Verifiable(Times.Once);
        return this;
    }

    /// <summary>
    /// Setup mock for `IClipboard.SetClipboardTextAsync`.
    /// </summary>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiFixture WithClipboardSetClipboardTextAsync(string semver)
    {
        _clipboardMock
            .Setup(c => c.SetClipboardTextAsync(semver))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);
        return this;
    }
}
