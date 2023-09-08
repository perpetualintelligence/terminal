/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    /// <summary>
    /// The <see cref="CommandRouteParser"/> class parses a command string into a <see cref="Root"/> object.
    /// </summary>
    public class CommandRouteParser : ICommandRouteParser
    {
        private readonly ITextHandler textHandler;
        private readonly ICommandStoreHandler commandStoreHandler;
        private readonly TerminalOptions terminalOptions;
        private readonly ILogger<CommandRouteParser> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRouteParser"/> class.
        /// </summary>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="commandStoreHandler">The command store handler.</param>
        /// <param name="terminalOptions">The terminal configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CommandRouteParser(ITextHandler textHandler, ICommandStoreHandler commandStoreHandler, TerminalOptions terminalOptions, ILogger<CommandRouteParser> logger)
        {
            this.textHandler = textHandler;
            this.commandStoreHandler = commandStoreHandler;
            this.terminalOptions = terminalOptions;
            this.logger = logger;
        }

        /// <summary>
        /// Parses the command string into a <see cref="Root"/> object.
        /// </summary>
        /// <param name="commandRoute"></param>
        /// <returns></returns>
        /// <exception cref="ErrorException"></exception>
        public async Task<ParsedCommand> ParseAsync(CommandRoute commandRoute)
        {
            // Initialize variables
            Root? root = null;
            Group? lastGroup = null;
            SubCommand? lastSubCommand = null;
            Command? lastCommand = null;
            List<string>? argValues = null;
            Dictionary<string, string>? optionPairs = null;
            bool argOrOptionsStarted = false;

            // Split the command string into elements using the regex pattern
            MatchCollection matches = Regex.Matches(commandRoute.Command.Raw, textHandler.ExtractionRegex(terminalOptions));
            for (int matchIdx = 0; matchIdx < matches.Count; ++matchIdx)
            {
                Match match = matches[matchIdx];
                string element = match.Value.Trim(terminalOptions.Extractor.Separator.ToArray());

                // Check if the element exists in the command dictionary. If it does not exist that means that the arguments and options have started
                // or the command id is invalid.
                CommandDescriptor? commandDescriptor = null;
                if (!argOrOptionsStarted)
                {
                    // Performance Improvement: The commandStoreHandler may involve a network call if we are querying a remote store. So we optimize the algorithm
                    // to query store till we start arguments and options processing.
                    try
                    {
                        commandDescriptor = await commandStoreHandler.FindByIdAsync(element);
                    }
                    catch (Exception ex)
                    {
                        logger.LogInformation(ex, "Failed to find command in the store. Assuming it to be an argument or an option. command={0}", element);

                        // The command is not in the store, either the command is invalid or the arguments and options have started.
                        argOrOptionsStarted = true;
                    }
                }

                if (!argOrOptionsStarted)
                {
                    // No exception was thrown but the command does not exist in the store.
                    if (commandDescriptor == null)
                    {
                        throw new ErrorException(TerminalErrors.UnsupportedCommand, "The command is not supported. command={0}", element);
                    }

                    Command command = new(commandDescriptor);
                    if (commandDescriptor.Type == CommandType.Root)
                    {
                        // There can be only root in the command route
                        if (root != null)
                        {
                            throw new ErrorException(TerminalErrors.InvalidCommand, "The command route contains multiple roots. root={0}", commandDescriptor.Id);
                        }

                        root = new Root(command);
                    }
                    else if (commandDescriptor.Type == CommandType.Group)
                    {
                        if (root == null)
                        {
                            throw new ErrorException(TerminalErrors.InvalidCommand, "The command group must be preceded by a root command. group={0}", commandDescriptor.Id);
                        }

                        Group group = new(command);
                        if (lastGroup == null)
                        {
                            root.ChildGroup = group;
                        }
                        else
                        {
                            lastGroup.ChildGroup = group;
                        }

                        lastGroup = group;
                    }
                    else if (commandDescriptor.Type == CommandType.SubCommand)
                    {
                        if (lastSubCommand != null)
                        {
                            throw new ErrorException(TerminalErrors.InvalidCommand, "The nested subcommands are not supported. command={0}", commandDescriptor.Id);
                        }

                        SubCommand subCommand = new(command);

                        if (lastGroup != null)
                        {
                            lastGroup.ChildSubCommand = subCommand;
                        }
                        else if (root != null)
                        {
                            root.ChildSubCommand = subCommand;
                        }

                        lastSubCommand = subCommand;
                    }

                    lastCommand = command;
                }
                else
                {
                    if (IsOption(element))
                    {
                        optionPairs ??= new Dictionary<string, string>();

                        int nextMatchIdx = matchIdx + 1;
                        if (nextMatchIdx < matches.Count)
                        {
                            string nextElement = matches[nextMatchIdx].Value.Trim(terminalOptions.Extractor.Separator.ToArray());
                            if (IsOption(nextElement))
                            {
                                //  Next element is an option so this element is a boolean option
                                optionPairs.Add(element, true.ToString());
                            }
                            else
                            {
                                optionPairs.Add(element, nextElement);
                                matchIdx = nextMatchIdx;
                            }
                        }
                    }
                    else
                    {
                        argValues ??= new List<string>();
                        argValues.Add(element);
                    }
                }
            }

            if (lastCommand == null)
            {
                throw new ErrorException(TerminalErrors.InvalidCommand, "The command string is missing a valid command. command_string={0}", commandRoute.Command.Raw);
            }

            // At the end we will either have a valid root or a default root.
            root ??= Root.Default();

            Arguments? arguments = null;
            if (argValues != null)
            {
                arguments = ParseArguments(lastCommand.Descriptor, argValues);
            }

            Options? options = null;
            if (optionPairs != null)
            {
                options = ParseOptions(lastCommand.Descriptor, optionPairs);
            }

            lastCommand.Arguments = arguments;
            lastCommand.Options = options;

            ParsedCommand extractedCommand = new(commandRoute, lastCommand, root);
            return extractedCommand;
        }

        private Options ParseOptions(CommandDescriptor descriptor, Dictionary<string, string> optValues)
        {
            Options options = new(textHandler);
            foreach (var optKvp in optValues)
            {
                bool found = descriptor.TryGetOptionDescriptor(optKvp.Key, out OptionDescriptor optionDescriptor);
                if (!found)
                {
                    throw new ErrorException(TerminalErrors.UnsupportedOption, "The command does not support option. command={0} option={1}", descriptor.Id, optKvp.Key);
                }

                options.Add(new Option(optionDescriptor, optKvp.Value));
            }
            return options;
        }

        private Arguments ParseArguments(CommandDescriptor commandDescriptor, List<string> values)
        {
            if (commandDescriptor.ArgumentDescriptors == null || !commandDescriptor.ArgumentDescriptors.Any())
            {
                throw new ErrorException(TerminalErrors.UnsupportedArgument, "The command does not support arguments. command={0}", commandDescriptor.Id);
            }

            if (commandDescriptor.ArgumentDescriptors.Count < values.Count)
            {
                throw new ErrorException(TerminalErrors.UnsupportedArgument, "The command does not support specified arguments. command={0} arguments={1}", commandDescriptor.Id, values.JoinBySpace());
            }

            Arguments arguments = new(textHandler);
            for (int idx = 0; idx < values.Count; ++idx)
            {
                Argument argument = new(commandDescriptor.ArgumentDescriptors[idx], values[idx]);
                arguments.Add(argument);
            }

            return arguments;
        }

        private bool IsOption(string value)
        {
            // Check if the match value starts with a prefix like "-" or "--"
            return value.StartsWith(terminalOptions.Extractor.OptionPrefix, textHandler.Comparison) ||
                   value.StartsWith(terminalOptions.Extractor.OptionAliasPrefix, textHandler.Comparison);
        }
    }
}