[![build-test-cross-manual](https://github.com/perpetualintelligence/cli/actions/workflows/build-test-cross-manual.yml/badge.svg)](https://github.com/perpetualintelligence/cli/actions/workflows/build-test-cross-manual.yml)
[![build-test-cross-manual](https://github.com/perpetualintelligence/cli/actions/workflows/build-test-cross-manual.yml/badge.svg)](https://github.com/perpetualintelligence/cli/actions/workflows/build-test-cross-manual.yml)
[![delete-packages](https://github.com/perpetualintelligence/cli/actions/workflows/delete-packages.yml/badge.svg)](https://github.com/perpetualintelligence/cli/actions/workflows/delete-packages.yml)

![macOS](https://img.shields.io/badge/macOS-Catalina%2010.15-blue?style=flat-square&logo=macos)
![ubuntu](https://img.shields.io/badge/linux-ubuntu--20.04-blue?style=flat-square&logo=ubuntu)
![windows](https://img.shields.io/badge/windows-2019-blue?style=flat-square&logo=windows)

> **Note:** This is an ***alpha*** release, not ready for production usage. It is also subject to design changes without any advance notice.

# STOP

> ***BEFORE MAKING THE REPO PUBLIC, UPDATE THE CICD PIPELINES TO USE THE ORGANIZATION SECRETS. CURRENTLY, WE DEFINE THEM IN THE cli REPO BECAUSE FREE TIER DOES NOT ALLOW ORG SECRETS FOR PRIVATE REPO.***
> 
> ***DELETE THE REPO SECRETS AND USE THE ORG SECRETS WHEN THIS REPO IS PUBLIC OR UPDATED TO PREMIUM.***

## Introduction

This repository contains the cross-platform cli framework. We build the following NuGet packages from this repository.

[![Nuget](https://img.shields.io/nuget/vpre/PerpetualIntelligence.Cli?label=PerpetualIntelligence.Cli)](https://www.nuget.org/packages/PerpetualIntelligence.Cli)

We track the [issues and tasks](https://github.com/perpetualintelligence/cli/issues) here. We make our best effort to respond to issues in a timely fashion. You can read more about our procedures for classifying and resolving issues in our [Issues policy](https://terms.perpetualintelligence.com/articles/issues-policy.html) topic.

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our community.
See the [Code of Conduct](https://terms.perpetualintelligence.com/articles/CODE_OF_CONDUCT.html).

## Environment
- [github-release](https://github.com/perpetualintelligence/cli/deployments/activity_log?environment=github-release): Tracks the release to publish the packages to [Nuget](https://www.nuget.org/profiles/perpetualintelligencellc) and [GitHub](https://github.com/orgs/perpetualintelligence/packages?repo_name=cli).

## Build

### Workflow
This workflow folder contains the build and deployment pipelines for generating and publishing [Nuget](https://www.nuget.org/profiles/perpetualintelligencellc) and [GitHub](https://github.com/orgs/perpetualintelligence/packages?repo_name=data) packages. 

The build and deployment include:
1. *build-test-ci*: The automated CI builds and tests the code changes.
2. *build-test-publish*: The manual release publishes the packages to [Nuget](https://www.nuget.org/profiles/perpetualintelligencellc) and [GitHub](https://github.com/orgs/perpetualintelligence/packages?repo_name=data), see [releases](https://github.com/perpetualintelligence/cli/releases)
3. *delete-packages*:  The automated action cleans the packages every week and keeps the latest working version. For stable versions, refer to [Nuget](https://www.nuget.org/profiles/perpetualintelligencellc) packages.

> **The manual release requires approval.**

### Package Versions
All packages follow [sematic](https://semver.org/) versioning schemes. The env file *package_version.env* defines the package versions.

### Project Dependencies
The *PI_CI_REFERENCE* environment variable defines how *.csproj* references the dependencies for CI and local development. It supportes the following values:
- *local*: Project references for local development within the same repo
- *cross*: Project references for local development across repos
- *package*: Package references for CI/CD and deployment

> PI_CI_REFERENCE environment variable (**local** or **cross**) needs to be set on dev machine . The **package** value is not supported on dev machine. 

### Composite Actions
The *push-package* composite action builds, tests, packs, and publishes the package to the feed.
