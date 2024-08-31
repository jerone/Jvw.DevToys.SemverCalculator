using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Performance",
    "CA1869",
    Justification = "Do not cache JsonSerializerOptions in tests."
)]
