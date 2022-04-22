# Build

## Workflow
This workflow folder contains the build and deployment pipelines for generating and publishing [Nuget](https://www.nuget.org/profiles/perpetualintelligencellc) and [GitHub](https://github.com/orgs/perpetualintelligence/packages?repo_name=data) packages. 

The build and deployment include:
1. *build-test-ci*: The automated CI builds and tests the code changes.
2. *build-test-publish*: The manual release publishes the packages to [Nuget](https://www.nuget.org/profiles/perpetualintelligencellc) and [GitHub](https://github.com/orgs/perpetualintelligence/packages?repo_name=data), see [releases](https://github.com/perpetualintelligence/cli/releases)
3. *delete-packages*:  The automated action cleans the packages every week and keeps the latest working version. For stable versions, refer to [Nuget](https://www.nuget.org/profiles/perpetualintelligencellc) packages.

> **The manual release requires approval.**

## Package Versions
All packages follow [sematic](https://semver.org/) versioning schemes. The env file *package_version.env* defines the package versions.

## Project Dependencies
The *PI_CI_REFERENCE* environment variable defines how *.csproj* references the dependencies for CI and local development. It supportes the following values:
- *local*: Project references for local development within the same repo
- *cross*: Project references for local development across repos
- *package*: Package references for CI/CD and deployment

> PI_CI_REFERENCE environment variable (**local** or **cross**) needs to be set on dev machine . The **package** value is not supported on dev machine. 

## Composite Actions
The *push-package* composite action builds, tests, packs, and publishes the package to the feed.
