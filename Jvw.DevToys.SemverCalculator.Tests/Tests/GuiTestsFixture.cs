using DevToys.Api;
using Jvw.DevToys.SemverCalculator.Models;
using Jvw.DevToys.SemverCalculator.Services;
using Moq;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests;

/// <summary>
/// Fixture for GUI tests.
/// </summary>
internal class GuiTestsFixture
{
    private readonly Mock<ISettingsProvider> _settingsProviderMock = new(MockBehavior.Strict);
    private readonly Mock<INpmService> _npmServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IVersionService> _versionServiceMock = new(MockBehavior.Strict);
    private Gui Sut { get; set; } = null!;

    /// <summary>
    /// Create the system under test.
    /// </summary>
    /// <returns>System under test.</returns>
    internal Gui CreateSut()
    {
        Sut = new Gui(
            _settingsProviderMock.Object,
            _npmServiceMock.Object,
            _versionServiceMock.Object
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

    /// <summary>
    /// Verify all mocks.
    /// </summary>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiTestsFixture VerifyAll()
    {
        _settingsProviderMock.VerifyAll();
        _settingsProviderMock.VerifyNoOtherCalls();
        _npmServiceMock.VerifyAll();
        _npmServiceMock.VerifyNoOtherCalls();
        _versionServiceMock.VerifyAll();
        _versionServiceMock.VerifyNoOtherCalls();
        return this;
    }

    /// <summary>
    /// Setup mock for `SettingsProvider.GetSetting` with return value.
    /// </summary>
    /// <typeparam name="T">Type of value of the setting.</typeparam>
    /// <param name="key">Setting key.</param>
    /// <param name="value">Setting value.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiTestsFixture WithSettingsProviderGetSettings<T>(SettingDefinition<T> key, T value)
    {
        _settingsProviderMock.Setup(x => x.GetSetting(key)).Returns(value).Verifiable(Times.Once);
        return this;
    }

    /// <summary>
    /// Setup mock for `SettingsProvider.SetSetting`.
    /// </summary>
    /// <typeparam name="T">Type of value of the setting.</typeparam>
    /// <param name="key">Setting key.</param>
    /// <param name="value">Setting value.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiTestsFixture WithSettingsProviderSetSettings<T>(SettingDefinition<T> key, T value)
    {
        _settingsProviderMock.Setup(x => x.SetSetting(key, value)).Verifiable(Times.Once);
        return this;
    }

    /// <summary>
    /// Setup mock for `NpmService.FetchPackage` with return value.
    /// </summary>
    /// <param name="packageName">Package name.</param>
    /// <param name="package">Package.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiTestsFixture WithNpmServiceFetchPackage(string packageName, PackageJson? package)
    {
        _npmServiceMock
            .Setup(x => x.FetchPackage(packageName))
            .ReturnsAsync(package)
            .Verifiable(Times.Once);
        return this;
    }

    /// <summary>
    /// Setup mock for `VersionService.SetVersions`.
    /// </summary>
    /// <param name="packageVersions">Package versions.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiTestsFixture WithVersionServiceSetVersions(List<string> packageVersions)
    {
        _versionServiceMock.Setup(x => x.SetVersions(packageVersions)).Verifiable(Times.Once);
        return this;
    }

    /// <summary>
    /// Setup mock for `VersionService.TryParseRange` with return value.
    /// </summary>
    /// <param name="range">Version range.</param>
    /// <param name="result">Result of the parsing.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiTestsFixture WithVersionServiceTryParseRange(string range, bool result)
    {
        _versionServiceMock
            .Setup(x => x.TryParseRange(range))
            .Returns(result)
            .Verifiable(Times.Once());
        return this;
    }

    /// <summary>
    /// Setup mock for `VersionService.MatchVersions` with return value.
    /// </summary>
    /// <param name="includePreReleases">Include pre-releases.</param>
    /// <param name="result">Result of the matching.</param>
    /// <param name="times">Optional verify times. Default is once.</param>
    /// <returns>This fixture, for chaining.</returns>
    internal GuiTestsFixture WithVersionServiceMatchVersions(
        bool includePreReleases,
        List<IUIElement> result,
        Times? times = null
    )
    {
        _versionServiceMock
            .Setup(x => x.MatchVersions(includePreReleases))
            .Returns(result)
            .Verifiable(times ?? Times.Once());
        return this;
    }
}
