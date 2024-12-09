# Build

## Local Machine
Follow the steps to set up the `oneimlx.terminal` repository on your local development machine.

1. Download and install [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)
2. Clone the [oneimlx.terminal](https://github.com/perpetualintelligence/terminal) GitHub repo
3. Set PI_CI_REFERENCE environment variable to `cross`
4. Set PI_CLI_TEST_LIC environment variable to your development license file location

## CICD
This workflow folder contains the build and deployment pipelines for generating and publishing [Nuget](https://www.nuget.org/profiles/perpetualintelligencellc) and [GitHub](https://github.com/orgs/perpetualintelligence/packages?repo_name=data) packages. 

- *build-test-cross-manual*: The manual action that builds and tests the code changes on Windows, Linux and macOS.
- *build-test-publish*: The automated action that publishes the packages to [Nuget](https://www.nuget.org/profiles/perpetualintelligencellc) and [GitHub](https://github.com/orgs/perpetualintelligence/packages?repo_name=data), see [releases](https://github.com/perpetualintelligence/cli/releases)
- *delete-packages*:  The automated action cleans the packages every week and keeps the latest working version. For stable versions, refer to [Nuget](https://www.nuget.org/profiles/perpetualintelligencellc) packages.

> ***Note: The `build-test-publish` release to Nuget pipeline triggers a deployment approval.***

## Package Versions
All packages follow [sematic](https://semver.org/) versioning schemes. The env file *package_version.env* defines the package versions.

## Project Dependencies
The *PI_CI_REFERENCE* environment variable defines how *.csproj* references the dependencies for CI and local development. It supportes the following values:
- *local*: Project references for local development within the same repo
- *cross*: Project references for local development across repos
- *package*: Package references for CI/CD and deployment

> PI_CI_REFERENCE environment variable (**local** or **cross**) needs to be set on dev machine . The **package** value is not supported on dev machine. 

## Composite Actions
The `push-package` composite action builds, tests, packs, and publishes the package to the feed.
