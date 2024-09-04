using System.Runtime.CompilerServices;
using Argon;

namespace Jvw.DevToys.SemverCalculator.Tests;

/// <summary>
/// Class for initializing the test module, like Verify tool.
/// </summary>
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
            // Export all properties, including those with default values.
            // This is to guard that the "contract" (what is output by the API) doesn't change.
            settings.DefaultValueHandling = DefaultValueHandling.Include
        );
    }
}