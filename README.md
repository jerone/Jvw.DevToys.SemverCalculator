# Jvw.DevToys.SemverCalculator

A Semantic Versioning ([SemVer](https://semver.org)) calculator for [DevToys](https://devtoys.app/) App.

![Screenshot of Jvw.DevToys.SemverCalculator](https://raw.githubusercontent.com/jerone/Jvw.DevToys.SemverCalculator/master/screenshot.jpg)

## Installation

1. Download the `Jvw.DevToys.SemverCalculator` NuGet package from [NuGet.org](https://www.nuget.org/packages/Jvw.DevToys.SemverCalculator/).
2. Open DevToys, go to `Manage extensions`, click on `Install an extension` and select the downloaded NuGet package.

## Agreement

To fetch the list of versions from a NPM package, this extension makes a GET HTTP request to the NPM registry.
No data is send to NPM, other than the package name.
By using this extension, you agree to the [NPM Terms of Use](https://www.npmjs.com/policies/terms).

## Limitations

No support for DevToys CLI (for now).

## Contributing

### Setup

1. Clone [the repository](https://github.com/jerone/Jvw.DevToys.SemverCalculator).
2. Follow the [instructions on DevToys.app](https://devtoys.app/doc/articles/extension-development/getting-started/setup.html) to run the project locally.
3. Press <kbd>F5</kbd> to start debugging.

### Translation

After setup, locate `Jvw.DevToys.SemverCalculator/Resources/Resources.resx` file in Visual Studio, and [add your locale with translations](https://learn.microsoft.com/en-us/visualstudio/ide/managing-application-resources-dotnet?view=vs-2022).

### Guidelines

This project uses [CSharpier](https://csharpier.com/) to format the code. Install all required DotNet tools with `dotnet tool restore`.

This project also uses an [EditorConfig](https://editorconfig.org/) file for consistent coding styles. Make sure your IDE supports this.

## License

This extension is licensed under the MIT License - see the [LICENSE](https://github.com/jerone/Jvw.DevToys.SemverCalculator/blob/master/LICENSE.md) file for details.
