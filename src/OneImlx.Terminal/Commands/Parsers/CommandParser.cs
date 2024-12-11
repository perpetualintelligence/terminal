/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Shared.Extensions;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;

namespace OneImlx.Terminal.Commands.Parsers
{
    /// <summary>
    /// The default <see cref="ICommandParser"/>.
    /// </summary>
    /// <seealso cref="ParserOptions.Separator"/>
    public class CommandParser : ICommandParser
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="terminalRequestParser">The terminal request parser.</param>
        /// <param name="textHandler"></param>
        /// <param name="commandStore"></param>
        /// <param name="terminalOptions"></param>
        /// <param name="logger">The logger.</param>
        public CommandParser(
            ITerminalRequestParser terminalRequestParser,
            ITerminalTextHandler textHandler,
            ITerminalCommandStore commandStore,
            IOptions<TerminalOptions> terminalOptions,
            ILogger<CommandParser> logger)
        {
            this.terminalRequestParser = terminalRequestParser;
            this.textHandler = textHandler;
            this.commandStore = commandStore;
            this.terminalOptions = terminalOptions;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task ParseCommandAsync(CommandRouterContext context)
        {
            logger.LogDebug("Parse request. request={0} raw={1}", context.Request.Id, context.Request.Raw);
            TerminalParsedRequest parsedOutput = await terminalRequestParser.ParseRequestAsync(context.Request);
            context.ParsedCommand = await MapParsedRequestAsync(context.Request, parsedOutput);
        }

        private async Task<(List<CommandDescriptor> parsedCommands, List<Argument> parsedArguments)> MapCommandAndArguments(TerminalParsedRequest parsedOutput)
        {
            List<CommandDescriptor> parsedCommands = [];
            List<Argument> parsedArguments = [];
            CommandDescriptor? parsedCommand = null;

            // Process tokens
            int argId = 0;
            foreach (string token in parsedOutput.Tokens)
            {
                if (await commandStore.TryFindByIdAsync(token, out CommandDescriptor? currentCommand))
                {
                    if (currentCommand == null)
                    {
                        throw new TerminalException(TerminalErrors.InvalidCommand, "The command is found in the store but returned null descriptor. command={0}", token);
                    }

                    if (parsedArguments.Count > 0)
                    {
                        throw new TerminalException(TerminalErrors.InvalidArgument, "The command is found in arguments. command={0}", token);
                    }

                    // Make sure the current command belongs to the right owner
                    CommandDescriptor? lastCommand = parsedCommands.LastOrDefault();
                    if (lastCommand != null)
                    {
                        if (currentCommand.OwnerIds == null)
                        {
                            throw new TerminalException(TerminalErrors.InvalidCommand, "The command does not define an owner. command={0}", currentCommand.Id);
                        }

                        if (!currentCommand.OwnerIds.Contains(lastCommand.Id))
                        {
                            throw new TerminalException(TerminalErrors.InvalidCommand, "The command owner is not valid. owner={0} command={1}", lastCommand.Id, currentCommand.Id);
                        }
                    }
                    else
                    {
                        if (currentCommand.OwnerIds != null)
                        {
                            throw new TerminalException(TerminalErrors.MissingCommand, "The command owner is missing. command={0}", currentCommand.Id);
                        }
                    }

                    // The parsedCommand is used to keep track of the last command that is used as the basis for parsing
                    // and validating the arguments.
                    parsedCommand = currentCommand;
                    parsedCommands.Add(currentCommand);
                }
                else
                {
                    if (parsedCommand == null)
                    {
                        throw new TerminalException(TerminalErrors.MissingCommand, "The arguments were provided, but no command was found or specified.");
                    }

                    if (parsedCommand.ArgumentDescriptors == null)
                    {
                        throw new TerminalException(TerminalErrors.UnsupportedArgument, "The command does not support arguments. command={0}", parsedCommand.Id);
                    }

                    int nextCount = parsedArguments.Count + 1;
                    if (parsedCommand.ArgumentDescriptors.Count < nextCount)
                    {
                        throw new TerminalException(TerminalErrors.UnsupportedArgument, "The command does not support {0} arguments. command={1}", nextCount, parsedCommand.Id);
                    }

                    ArgumentDescriptor argumentDescriptor = parsedCommand.ArgumentDescriptors[argId];
                    Argument argument = new(argumentDescriptor, token);
                    parsedArguments.Add(argument);
                    argId++;
                }
            }

            return (parsedCommands, parsedArguments);
        }

        private Options? MapOptions(CommandDescriptor commandDescriptor, Dictionary<string, ValueTuple<string, bool>>? parsedOptions)
        {
            if (parsedOptions == null || parsedOptions.Count == 0)
            {
                return null;
            }

            if (commandDescriptor.OptionDescriptors == null || commandDescriptor.OptionDescriptors.Count == 0)
            {
                throw new TerminalException(TerminalErrors.UnsupportedOption, "The command does not support any options. command={0}", commandDescriptor.Id);
            }

            if (commandDescriptor.OptionDescriptors.Count < parsedOptions.Count)
            {
                throw new TerminalException(TerminalErrors.UnsupportedOption, "The command does not support {0} options. command={1} options={2}", parsedOptions.Count, commandDescriptor.Id, parsedOptions.Keys.JoinByComma());
            }

            // 1. An input can be either an option or an alias, but not both.
            // 2. If a segment is identified as an option, it must match the option ID.
            // 3. If identified as an alias, it must match the alias.
            List<Option> options = new(parsedOptions.Count);
            foreach (var optKvp in parsedOptions)
            {
                string optionOrAliasKey = optKvp.Key;
                if (!commandDescriptor.OptionDescriptors.TryGetValue(optionOrAliasKey, out var optionDescriptor))
                {
                    throw new TerminalException(TerminalErrors.UnsupportedOption, "The command does not support option or its alias. command={0} option={1}", commandDescriptor.Id, optionOrAliasKey);
                }

                string optionValue = optKvp.Value.Item1;
                bool isAlias = optKvp.Value.Item2;
                if (isAlias)
                {
                    // Validate if option matches expected alias
                    if (!textHandler.TextEquals(optionDescriptor.Alias, optionOrAliasKey))
                    {
                        throw new TerminalException(TerminalErrors.InvalidOption, "The alias prefix is not valid for an option. alias={0} option={1}", optionDescriptor.Alias, optKvp.Key);
                    }
                }
                else
                {
                    // Validate if option matches expected id
                    if (!textHandler.TextEquals(optionDescriptor.Id, optionOrAliasKey))
                    {
                        throw new TerminalException(TerminalErrors.InvalidOption, "The option prefix is not valid for an alias. option={0} alias={1}", optionDescriptor.Id, optionOrAliasKey);
                    }
                }

                options.Add(new Option(optionDescriptor, optionValue, isAlias));
            }

            return new Options(textHandler, options);
        }

        private async Task<ParsedCommand> MapParsedRequestAsync(TerminalRequest request, TerminalParsedRequest parsedOutput)
        {
            // Map to command and arguments
            var (commandDescriptors, parsedArguments) = await MapCommandAndArguments(parsedOutput);

            // Process Options
            CommandDescriptor commandDescriptor = commandDescriptors.Last();
            Options? parsedOptions = MapOptions(commandDescriptor, parsedOutput.Options);

            // Final result.
            Arguments? arguments = null;
            if (parsedArguments.Count > 0)
            {
                arguments = new Arguments(textHandler, parsedArguments);
            }

            // Hierarchy is all expect the current command.
            Command command = new(commandDescriptor, arguments, parsedOptions);
            return new ParsedCommand(command, commandDescriptors.Count > 1 ? commandDescriptors.Take(commandDescriptors.Count - 1) : null);
        }

        private readonly ITerminalCommandStore commandStore;
        private readonly ILogger<CommandParser> logger;
        private readonly IOptions<TerminalOptions> terminalOptions;
        private readonly ITerminalRequestParser terminalRequestParser;
        private readonly ITerminalTextHandler textHandler;
    }
}
