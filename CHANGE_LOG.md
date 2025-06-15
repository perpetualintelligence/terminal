# 6.2.x
> Feature Change | Breaking Change
- Introduce gRPC terminal routing
- Introduce HTTP terminal routing
- Introduce `OneImlx.Terminal.Shared` for shared terminal services
- Introduce `OneImlx.Terminal.Server` for hosting terminal servers in ASP.NET Core
- Introduce `OneImlx.Terminal.Client` for client application communicating with terminal servers
- Update licensing and pricing
- Rename `Integration` to `Dynamics` for dynamic loading of commands
- Simplify terminal startup and use IHostApplicationLifetime for graceful shutdown

# 5.12.3
> Breaking Change
- Rename `TerminalExceptionHandler` to `TerminalLoggerExceptionHandler` that logs the error message to ILogger
- Add `TerminalConsoleExceptioneHandler` that logs the error message to ITerminalConsole
- Rename `TerminalHelpLoggerProvider` to `TerminalLoggerHelpProvider`
- Rename `TerminalHelpConsoleProvider` to `TerminalConsoleHelpProvider`
- Add custom properties for each command request 

# 5.11.1
- Production release.

# 5.11.1-rc*
> Feature Change
- Support UDP router `TerminalUdpRouter` for terminal routing
- Add `TerminalServices` for common terminal services
- Fixed TCP bug to process the messages as they come

# 5.9.1-rc*
> Breaking Change
- Support multi-targets net481, net8.0, netstandard2.0 and netstandard2.1
- Simplify licensing based on tenant and single key
- Support On-Prem terminals for secured environments
- Move `ITextHandler` to `Runtime` namespace
- Added `apps` for hosting test applications
- Merge application identifier as terminal identifier
- Merge Mutable and Immutable store into 1 common `ITerminalCommandStore`
- Remove redundant `CommandRunnerAttribute` and rename declarative target to declarative runner

# 5.6.4-rc*
> Breaking Change
- Use `OneImlx.Shared` and `OneImlx.Test` pacakges. 
- Migrate all tests from MSTest to xUnit

# 5.6.1-rc*
> Breaking Change
- Rename the package to `OneImlx.Terminal`
- Add `OneImlx.Terminal.Authentication` with MSAL public client support 
- Update AddTextHandler service method to accept the instance.
- Remove HttpOptions and move the options to their respective usage

# 5.4.1-rc*
> Breaking Change
- Support mutable and immutable store
- Support integration namespace for loading commands from an external source
- Pre-Production release for `netstandard2.0` and `netstandard2.1`
- Unit test on `net7`
- Merge TerminalStartInfo into TerminalStartContext
- Support cancellation for Terminal  Routing and Command Routing independently
- Rename `IAsyncEventHandler` to `ITerminalEventHandler` for clarity

# 5.3.1-rc*
> Breaking Change
- Rename command extractor types to command parser
- Remove redundant terminal routing result.
- Support On-Premise licensing for OSATs, factories and secured facilities that are not connected to public internet
- Upgrade the licensing
- Removed logging specific options in favor of ILogger<T>
- Merge `CommandString` into `CommandRoute`

# 5.2.1-rc*
> Breaking Change
- Disable command check during help execution
- Rename various abstractions to reflect the exact purpose (e.g. `HandleAsync` -> `HandleCommandAsync`)

# 5.1.1-rc*
> Breaking Change
- Add IHost terminal routing extension method
- Support Arguments and Options parsing
- Rename extractor and router options
- Remove default option and default option value feature in favor of command argument feature
- Remove `IErrorHandler` in favor of `IExceptionHandler`
- Rename `IRoutingService` to `ITerminalRouting`
- Move routing service to runtime namespace
- Introduce ITerminalConsole abstraction
- Rename Errors, Handlers
- Remove ConsoleHelper

# 5.0.1-rc*
> Breaking Change
- Remove the default option feature
- Introduce the command argument feature
- Introduce the terminal driver feature 
- Enforce terminal identifier

# 4.5.2-rc*
> Breaking Change
- Change the default values for `OptionPrefix`, `OptionAliasPrefix`, and `OptionValueSeparator`
- Initial support for on-premise licensing 

# 4.5.1-rc*
> Breaking Change
- Rename `Cli` namespace to `Terminal`
- Rename `CliOptions` to `TerminalOptions`
- Rename `*CliBuilder` to `*TerminalBuilder`
- Rename CICI actions for `PerpetualIntelligence.Cli` to `PerpetualIntelligence.Terminal`
- Deprecate `PerpetualIntelligence.Cli` in favor of `PerpetualIntelligence.Terminal`

# 4.4.2-rc*
> Breaking Change
- Rename all classes with `Cli*` to `Terminal*`
- Rename all classes with `Argument*` to `Option*`
- Rename Router Remote Options
- Check maximum command string length in router
- Update Demo license schema

# v4.3.2-rc*
- Add TCP IP routing service
- Port old code base and migrate to .NET Standard2.0, ,NET Standard2.1 and xUnit Tests for .NET7

# v3.0.0
- Initial release of the terminal Nuget packages

# v2.0.0
- Internal release for general framework

# v1.0.0
- Initial release for internal CLI routing
