namespace Jvw.DevToys.SemverCalculator.Tests;

/// <summary>
/// Base interface for fixtures.
/// </summary>
/// <typeparam name="TSut">Type of system under test (SUT).</typeparam>
/// <typeparam name="TFixture">Type of fixture belonging to SUT.</typeparam>
internal interface IBaseFixture<out TSut, out TFixture>
    where TSut : class
    where TFixture : class, IBaseFixture<TSut, TFixture>
{
    /// <summary>
    /// Create the system under test (SUT).
    /// </summary>
    /// <returns>System under test.</returns>
    internal TSut CreateSut();

    /// <summary>
    /// Verify all mocks.
    /// </summary>
    /// <returns>This fixture, for chaining.</returns>
    internal TFixture VerifyAll();
}
