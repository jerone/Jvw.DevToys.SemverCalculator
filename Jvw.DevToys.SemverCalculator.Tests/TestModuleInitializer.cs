using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Argon;
using Jvw.DevToys.SemverCalculator.Tests.Converters;

namespace Jvw.DevToys.SemverCalculator.Tests;

/// <summary>
/// Class for initializing the test module, like Verify tool.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Part of unit-testing.")]
public static class TestModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // Don't scrub DateTimes.
        VerifierSettings.DontScrubDateTimes();

        // Don't scrub Guids.
        VerifierSettings.DontScrubGuids();

        VerifierSettings.AddExtraSettings(settings =>
        {
            // Export all properties, including those with default values.
            // This is to guard that the "contract" doesn't change.
            settings.DefaultValueHandling = DefaultValueHandling.Include;

            // Handle DevToys elements.
            settings.Converters.Add(new DevToysElementConverter());
        });
    }
}
