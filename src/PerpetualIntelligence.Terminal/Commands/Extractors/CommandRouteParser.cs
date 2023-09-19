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
using System.Text;
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
        /// Parses the command string into a <see cref="ParsedCommand"/> object.
        /// </summary>
        /// <param name="commandRoute">The command route to parse.</param>
        /// <returns>A parsed command derived from the provided command route.</returns>
        /// <remarks>
        /// This method leverages a Queue data structure to facilitate and streamline the parsing process.
        /// Using a Queue aids in making the parsing sequence more predictable and the code more readable.
        /// <para>Performance Insight:</para>
        /// <list type="bullet">
        /// <item>
        /// <description>Direct Iteration: Time complexity is O(n), where n represents the number of segments in the command string.</description>
        /// </item>
        /// <item>
        /// <description>Using Queue: While the time complexity technically becomes O(2n) due to the enqueue and dequeue operations, it's asymptotically equivalent to O(n). Nevertheless, there is a slight overhead attributed to the enqueue and dequeue operations.</description>
        /// </item>
        /// </list>
        /// In practical applications with typical command string sizes, the differences in performance between the two methods are expected to be minimal.
        /// </remarks>
        public async Task<ParsedCommand> ParseAsync(CommandRoute commandRoute)
        {
            // Segment the raw command into individual components
            Queue<string> segmentsQueue = new(commandRoute.Command.Raw.Split(new[] { terminalOptions.Extractor.Separator }, StringSplitOptions.None));

            // Handle the processing of commands and arguments
            (List<CommandDescriptor> parsedCommands, List<string> parsedArguments) = await ProcessCommandsAndArgumentsAsync(segmentsQueue);

            // Extract and process command options
            Dictionary<string, string> parsedOptions = ProcessOptions(segmentsQueue);

            // Log parsed details if debug level logging is enabled
            LogIfDebugLevelEnabled(parsedCommands, parsedArguments, parsedOptions);

            // Compile and return the parsed command details
            return ParseCommand(commandRoute, parsedCommands, parsedArguments, parsedOptions);
        }

        private Dictionary<string, string> ProcessOptions(Queue<string> segmentsQueue)
        {
            string delimiter = terminalOptions.Extractor.ValueDelimiter;
            string separator = terminalOptions.Extractor.Separator;
            Dictionary<string, string> parsedOptions = new();

            while (segmentsQueue.Count > 0)
            {
                string segment = segmentsQueue.Dequeue();
                StringBuilder optionValueBuilder = new();

                // Collect option values
                while (segmentsQueue.Any() && !IsOption(segmentsQueue.Peek()))
                {
                    if (optionValueBuilder.Length > 0)
                    {
                        optionValueBuilder.Append(separator); // Using space as a separator
                    }
                    optionValueBuilder.Append(segmentsQueue.Dequeue());
                }

                // Process the option value and trim the end of the value if it ends with the separator.
                string optionValue = optionValueBuilder.Length > 0 ? optionValueBuilder.ToString().TrimEnd(separator, textHandler.Comparison) : true.ToString();
                if (StartsWith(optionValue, delimiter))
                {
                    optionValue = RemovePrefix(optionValue, delimiter);
                }
                if (EndsWith(optionValue, delimiter))
                {
                    optionValue = RemoveSuffix(optionValue, delimiter);
                }

                // Get the option or alias
                string? optionOrAlias;
                if (IsOptionPrefix(segment))
                {
                    optionOrAlias = RemovePrefix(segment, terminalOptions.Extractor.OptionPrefix);
                }
                else if (IsAliasPrefix(segment))
                {
                    optionOrAlias = RemovePrefix(segment, terminalOptions.Extractor.OptionAliasPrefix);
                }
                else
                {
                    throw new ErrorException(TerminalErrors.InvalidOption, "The option is missing the prefix. option={0}", segment);
                }

                parsedOptions[optionOrAlias] = optionValue;
            }

            return parsedOptions;
        }

        private async Task<(List<CommandDescriptor> ParsedDescriptors, List<string> ParsedArguments)> ProcessCommandsAndArgumentsAsync(Queue<string> segmentsQueue)
        {
            string delimiter = terminalOptions.Extractor.ValueDelimiter;
            string separator = terminalOptions.Extractor.Separator;

            List<CommandDescriptor> parsedDescriptors = new();
            List<string> parsedArguments = new();
            string? potentialLastCommandId = null;

            while (segmentsQueue.Count > 0)
            {
                // Break loop if segment represents an option.
                string segment = segmentsQueue.Peek();
                if (IsOption(segment))
                {
                    break;
                }
                segmentsQueue.Dequeue();

                // If we are not within a delimiter then we cannot have a separator.
                if (segment.IsNullOrEmpty())
                {
                    continue;
                }

                if (await commandStoreHandler.TryFindByIdAsync(segment, out CommandDescriptor? currentDescriptor))
                {
                    if (currentDescriptor == null)
                    {
                        throw new ErrorException(TerminalErrors.ServerError, "Command found in the store but returned null descriptor.");
                    }

                    if (!parsedArguments.IsNullOrEmpty())
                    {
                        potentialLastCommandId = parsedArguments.Last();
                    }
                }
                else
                {
                    StringBuilder argumentValueBuilder = new(segment, segment.Length + 10);

                    if (StartsWith(segment, delimiter))
                    {
                        while (segmentsQueue.Count > 0 && (!EndsWith(segment, delimiter) || argumentValueBuilder.Length == 1))
                        {
                            argumentValueBuilder.Append(separator);
                            segment = segmentsQueue.Dequeue();
                            argumentValueBuilder.Append(segment);
                        }

                        if (!EndsWith(segment, delimiter))
                        {
                            throw new ErrorException(TerminalErrors.InvalidCommand, "The argument value is missing the closing delimiter. argument={0}", argumentValueBuilder.ToString());
                        }
                    }

                    string argumentValue = argumentValueBuilder.ToString();
                    if (StartsWith(argumentValue, delimiter))
                    {
                        argumentValue = RemovePrefix(argumentValue, delimiter);
                    }
                    if (EndsWith(argumentValue, delimiter))
                    {
                        argumentValue = RemoveSuffix(argumentValue, delimiter);
                    }
                    parsedArguments.Add(argumentValue);
                }

                if (currentDescriptor != null)
                {
                    if (potentialLastCommandId != null && (currentDescriptor.Owners == null || !currentDescriptor.Owners.Contains(potentialLastCommandId)))
                    {
                        throw new ErrorException(TerminalErrors.InvalidCommand, "The command owner is not valid. owner={0} command={1}.", potentialLastCommandId, currentDescriptor.Id);
                    }
                    else if (potentialLastCommandId == null && currentDescriptor.Owners != null && currentDescriptor.Owners.Any())
                    {
                        throw new ErrorException(TerminalErrors.MissingCommand, "The command owner is missing in the command route. owners={0} command={1}.", currentDescriptor.Owners.JoinBySpace(), currentDescriptor.Id);
                    }

                    potentialLastCommandId = segment;
                    parsedDescriptors.Add(currentDescriptor);
                }
            }

            return (parsedDescriptors, parsedArguments);
        }

        private void LogIfDebugLevelEnabled(List<CommandDescriptor> parsedDescriptors, IEnumerable<string> parsedArguments, Dictionary<string, string> parsedOptions)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            StringBuilder debugLog = new();

            debugLog.AppendLine("Commands:");
            foreach (var cmd in parsedDescriptors)
            {
                debugLog.AppendLine(cmd.Id);
            }

            debugLog.AppendLine("Arguments:");
            foreach (var arg in parsedArguments)
            {
                debugLog.AppendLine(arg);
            }

            debugLog.AppendLine("Options:");
            foreach (var opt in parsedOptions)
            {
                debugLog.AppendLine($"{opt.Key}={opt.Value}");
            }

            logger.LogDebug(debugLog.ToString());
        }

        private ParsedCommand ParseCommand(CommandRoute commandRoute, List<CommandDescriptor> parsedDescriptors, List<string>? parsedArguments, Dictionary<string, string>? parsedOptions)
        {
            if (!parsedDescriptors.Any())
            {
                throw new ErrorException(TerminalErrors.MissingCommand, "The command is missing in the command route.");
            }

            // The last command in the route is the one that will be executed
            CommandDescriptor executingCommandDescriptor = parsedDescriptors[parsedDescriptors.Count - 1];
            Command executingCommand = new(
                executingCommandDescriptor,
                ParseArguments(executingCommandDescriptor, parsedArguments)
,
                ParseOptions(executingCommandDescriptor, parsedOptions));

            // Build the hierarchy and return the parsed command
            return new ParsedCommand(commandRoute, executingCommand, BuildHierarchy(parsedDescriptors, executingCommand));
        }

        private Root? BuildHierarchy(List<CommandDescriptor> parsedDescriptors, Command executingCommand)
        {
            if (!terminalOptions.Extractor.ParseHierarchy.GetValueOrDefault())
            {
                return null;
            }

            Root? hierarchy = null;
            Group? lastGroup = null;
            SubCommand? lastSubCommand = null;
            foreach (CommandDescriptor currentDescriptor in parsedDescriptors)
            {
                Command currentCommand = textHandler.TextEquals(currentDescriptor.Id, executingCommand.Id)
                                         ? executingCommand
                                         : new Command(currentDescriptor);

                switch (currentDescriptor.Type)
                {
                    case CommandType.Root:
                        {
                            if (hierarchy != null)
                            {
                                throw new ErrorException(TerminalErrors.InvalidCommand, "The command route contains multiple roots. root={0}", currentDescriptor.Id);
                            }

                            hierarchy = new Root(currentCommand);
                            break;
                        }

                    case CommandType.Group:
                        {
                            Group group = new(currentCommand);
                            if (lastGroup == null)
                            {
                                if (hierarchy == null)
                                {
                                    throw new ErrorException(TerminalErrors.MissingCommand, "The command group is missing a root command. group={0}", currentCommand.Id);
                                }
                                hierarchy.ChildGroup = group;
                            }
                            else
                            {
                                lastGroup.ChildGroup = group;
                            }

                            lastGroup = group;
                            break;
                        }

                    case CommandType.SubCommand:
                        {
                            if (lastSubCommand != null)
                            {
                                throw new ErrorException(TerminalErrors.InvalidRequest, "The subcommands cannot be nested. command={0}", currentDescriptor.Id);
                            }

                            SubCommand subCommand = new(currentCommand);
                            if (lastGroup != null)
                            {
                                lastGroup.ChildSubCommand = subCommand;
                            }
                            else if (hierarchy != null)
                            {
                                hierarchy.ChildSubCommand = subCommand;
                            }
                            else
                            {
                                hierarchy = Root.Default(subCommand);
                            }
                            lastSubCommand = subCommand;
                            break;
                        }

                    default:
                        {
                            throw new ErrorException(TerminalErrors.InvalidRequest, "The command descriptor type is not valid. type={0}", currentDescriptor.Type);
                        }
                }
            }
            return hierarchy;
        }

        private Options? ParseOptions(CommandDescriptor commandDescriptor, Dictionary<string, string>? parsedOptions)
        {
            if (parsedOptions == null || parsedOptions.Count == 0)
            {
                return null;
            }

            if (commandDescriptor.OptionDescriptors == null || commandDescriptor.OptionDescriptors.Count == 0)
            {
                throw new ErrorException(TerminalErrors.UnsupportedArgument, "The command does not support options. command={0}", commandDescriptor.Id);
            }

            List<Option> options = new(parsedOptions.Count);
            foreach (var optKvp in parsedOptions)
            {
                options.Add(new Option(commandDescriptor.OptionDescriptors[optKvp.Key], optKvp.Value));
            }

            return new Options(textHandler, options);
        }

        private Arguments? ParseArguments(CommandDescriptor commandDescriptor, List<string>? parsedArguments)
        {
            if (parsedArguments == null || parsedArguments.Count == 0)
            {
                return null;
            }

            if (commandDescriptor.ArgumentDescriptors == null || commandDescriptor.ArgumentDescriptors.Count == 0)
            {
                throw new ErrorException(TerminalErrors.UnsupportedArgument, "The command does not support any arguments. command={0}", commandDescriptor.Id);
            }

            if (commandDescriptor.ArgumentDescriptors.Count < parsedArguments.Count)
            {
                throw new ErrorException(TerminalErrors.UnsupportedArgument, "The command does not support {0} arguments. command={1} arguments={2}", parsedArguments.Count, commandDescriptor.Id, parsedArguments.JoinByComma());
            }

            List<Argument> arguments = new(parsedArguments.Count);
            for (int idx = 0; idx < parsedArguments.Count; ++idx)
            {
                arguments.Add(new(commandDescriptor.ArgumentDescriptors[idx], parsedArguments[idx]));
            }
            return new Arguments(textHandler, arguments);
        }

        private bool IsOption(string value)
        {
            // Check if the match value starts with a prefix like "-" or "--"
            return IsOptionPrefix(value) || IsAliasPrefix(value);
        }

        private bool IsOptionPrefix(string value)
        {
            return StartsWith(value, terminalOptions.Extractor.OptionPrefix);
        }

        private bool IsAliasPrefix(string value)
        {
            return StartsWith(value, terminalOptions.Extractor.OptionAliasPrefix);
        }

        private string RemovePrefix(string value, string prefix)
        {
            return value.Substring(prefix.Length);
        }

        private string RemoveSuffix(string value, string suffix)
        {
            return value.Substring(0, value.Length - suffix.Length);
        }

        private bool StartsWith(string value, string prefix)
        {
            return value.StartsWith(prefix, textHandler.Comparison);
        }

        private bool EndsWith(string value, string suffix)
        {
            return value.EndsWith(suffix, textHandler.Comparison);
        }
    }
}