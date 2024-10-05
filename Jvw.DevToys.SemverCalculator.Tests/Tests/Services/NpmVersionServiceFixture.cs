using Jvw.DevToys.SemverCalculator.Services;

namespace Jvw.DevToys.SemverCalculator.Tests.Tests.Services;

/// <summary>
/// Fixture for NPM version service tests.
/// </summary>
internal class NpmVersionServiceFixture : IBaseFixture<NpmVersionService, NpmVersionServiceFixture>
{
    /// <inheritdoc cref="IBaseFixture{TSut,TFixture}.CreateSut" />
    public NpmVersionService CreateSut()
    {
        return new NpmVersionService();
    }

    /// <inheritdoc cref="IBaseFixture{TSut,TFixture}.VerifyAll" />
    public NpmVersionServiceFixture VerifyAll()
    {
        return this;
    }
}
