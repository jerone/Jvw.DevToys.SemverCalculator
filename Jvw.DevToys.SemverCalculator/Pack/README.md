# Why

All files in this folder are required to create a valid NuGet package.
They are automatically included in the NuGet package when the project is built.
See the `.csproj` file how.

## Semver

Semver is a dependency of this package.
The NuGet package does not come with a `.pdb` and `.xml` (documentation) file.
Both of which are required to build a valid NuGet package (for Source Link & Deterministic builds).
These files can be downloaded here: https://www.nuget.org/packages/Semver/2.3.0
