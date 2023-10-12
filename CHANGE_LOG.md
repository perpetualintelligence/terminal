# 5.3.1-rc*
> Breaking Change
- Support On-Premise licensing for OSATs, factories and secured facilities that are not connected to public internet
- Upgrade the licensing
- Removed logging specific options in favor of ILogger<T>

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
