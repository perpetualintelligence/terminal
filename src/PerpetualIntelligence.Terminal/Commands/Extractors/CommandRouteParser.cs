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
        /// Parses the command string into a <see cref="Root"/> object.
        /// </summary>
        /// <remarks>
        /// This method uses a Queue data structure to simplify the logic flow, making the code more readable and maintainable.
        /// The benefit of using a Queue is more on the side of code clarity and predictability of the sequence in which segments are processed.
        ///
        /// <para>From a performance standpoint:</para>
        /// <list type="bullet">
        /// <item>
        /// <description>Direct Iteration: Time complexity is O(n), where n is the number of segments.</description>
        /// </item>
        /// <item>
        /// <description>With Queue: Time complexity is technically O(2n) due to the enqueue and dequeue operations, which simplifies to O(n) in big O notation. But there's an overhead due to the enqueue and dequeue operations.</description>
        /// </item>
        /// </list>
        /// For most real-world scenarios with reasonably sized command strings, the difference in raw execution time between the two methods would likely be negligible.
        /// </remarks>

        public async Task<ParsedCommand> ParseAsync(CommandRoute commandRoute)
        {
            // Initialize variables
            string delimiter = terminalOptions.Extractor.ValueDelimiter;
            string separator = terminalOptions.Extractor.Separator;
            string prefix = terminalOptions.Extractor.OptionPrefix;
            string aliasPrefix = terminalOptions.Extractor.OptionAliasPrefix;

            Queue<string> segmentsQueue = new(commandRoute.Command.Raw.Split(new[] { separator }, StringSplitOptions.None));
            List<string> parsedCommands = new();
            List<string> parsedArguments = new();
            Dictionary<string, string> parsedOptions = new();

            // -----------------------------
            // Command Processing
            // -----------------------------
            // The initial segments of the input are primarily processed as commands.
            // Segments are dequeued from the queue and checked for their validity as commands.
            // If a segment isn't recognized as a valid command, it's treated as an argument or an option.
            // This approach is rooted in the expected input order:
            // commands -> arguments (optional) -> options (optional).
            // If a command is found after recognizing segments as arguments, those arguments
            // are treated as potential commands, aiding in better error handling and feedback.
            List<CommandDescriptor> commandDescriptors = new();
            List<string> potentialCommands = new();
            while (segmentsQueue.Any())
            {
                string segment = segmentsQueue.Peek();
                if (IsOption(segment))
                {
                    break;
                }
                segmentsQueue.Dequeue();

                // If the segment is a valid command, then we add it to the list of parsed commands. Otherwise we assume its an argument or an option.
                bool found = await commandStoreHandler.TryFindByIdAsync(segment, out CommandDescriptor? currentDescriptor);
                if (found)
                {
                    // If the command is found then we should also have a valid descriptor.
                    if (currentDescriptor == null)
                    {
                        throw new ErrorException(TerminalErrors.ServerError, "Command found in the store but returned null descriptor.");
                    }

                    // Arguments are specified after commands. If we found a command after arguments then we add the arguments to the list
                    // of potential commands. This enables us to provide better error messages when the user enters an invalid command.
                    if (parsedArguments.Any())
                    {
                        potentialCommands.AddRange(parsedArguments);
                        parsedArguments.Clear();
                    }
                }
                else
                {
                    // If we are here then that means the next segment is an argument.
                    parsedArguments ??= new List<string>();

                    // If the segment starts with a delimiter, process it as a potentially multi-segment argument.
                    // Otherwise, treat it as a single word argument.
                    StringBuilder argumentValueBuilder = new(segment);
                    if (StartsWith(segment, delimiter))
                    {
                        // The loop aggregates segments to form a complete argument value encapsulated by delimiters.
                        // While there are segments to process, the loop appends them to the current argument unless a delimiter signals the end.
                        // The argumentValueBuilder.Length == 1 check ensures that if a segment is only the starting delimiter, subsequent segments
                        // are appended until a closing delimiter is found.
                        while (segmentsQueue.Any() && (!EndsWith(segment, delimiter) || argumentValueBuilder.Length == 1))
                        {
                            // Since we are within the delimiter, we can safely append a separator
                            argumentValueBuilder.Append(separator);
                            segment = segmentsQueue.Dequeue();
                            argumentValueBuilder.Append(segment);
                        }

                        // Check if the closing delimiter was found.
                        if (!EndsWith(segment, delimiter))
                        {
                            throw new ErrorException(TerminalErrors.InvalidCommand, "The argument value is missing the closing delimiter. argument={0}", argumentValueBuilder.ToString());
                        }
                    }

                    // Trim delimiters from the argument value.
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

                // Check if the current command has a valid owner specified in the command route.
                if (currentDescriptor != null)
                {
                    string? previousCommandId = potentialCommands.LastOrDefault();
                    if (previousCommandId != null)
                    {
                        if (currentDescriptor.Owners == null || !currentDescriptor.Owners.Contains(previousCommandId))
                        {
                            throw new ErrorException(TerminalErrors.InvalidCommand, "The command owner is not valid. owner={0} command={1}.", previousCommandId, currentDescriptor.Id);
                        }
                    }
                    else
                    {
                        if (currentDescriptor.Owners != null && currentDescriptor.Owners.Any())
                        {
                            throw new ErrorException(TerminalErrors.MissingCommand, "The command owner is missing in the command route. owners={0} command={1}.", currentDescriptor.Owners.JoinBySpace(), currentDescriptor.Id);
                        }
                    }

                    potentialCommands.Add(segment);
                    commandDescriptors.Add(currentDescriptor);
                }
            }

            // -----------------------------
            // Option Processing
            // -----------------------------
            // After processing the arguments, we'll treat the remaining segments as options.
            // Each option starts with a prefix (like '--' or '-') followed by its associated
            // value(s). The value for an option is the concatenation of all following segments
            // until the next option or until the end of the input.
            while (segmentsQueue.Any())
            {
                string segment = segmentsQueue.Dequeue();
                StringBuilder optionValueBuilder = new();

                // Option value can be a single word, multiple words, or a delimited value.
                // This loop will continue until we encounter the next option or until we reached the end of the queue.
                while (segmentsQueue.Any() && !IsOption(segmentsQueue.Peek()))
                {
                    if (optionValueBuilder.Length > 0)
                    {
                        optionValueBuilder.Append(separator); // Using space as a separator
                    }
                    optionValueBuilder.Append(segmentsQueue.Dequeue());
                }

                // If the option value is delimited then we remove the delimiters.
                string optionValue = optionValueBuilder.Length > 0 ? optionValueBuilder.ToString() : true.ToString();
                if (StartsWith(optionValue, delimiter))
                {
                    optionValue = RemovePrefix(optionValue, delimiter);
                }
                if (EndsWith(optionValue, delimiter))
                {
                    optionValue = RemoveSuffix(optionValue, delimiter);
                }

                string? optionOrAlias = null;
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

            // Log for debug
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Commands:");
                parsedCommands.ForEach(cmd => logger.LogDebug($"{cmd}{Environment.NewLine}"));

                logger.LogDebug("\nArguments:");
                parsedArguments.ForEach(arg => logger.LogDebug($"{arg}{Environment.NewLine}"));

                logger.LogDebug("\nOptions:");
                foreach (var opt in parsedOptions)
                {
                    logger.LogDebug($"{opt.Key}={opt.Value}{Environment.NewLine}");
                }
            }

            ParsedCommand parsedCommand = await ParseCommandAsync(commandRoute, commandDescriptors, parsedArguments, parsedOptions);
            return parsedCommand;
        }

        private Task<ParsedCommand> ParseCommandAsync(CommandRoute commandRoute, List<CommandDescriptor> parsedDescriptors, List<string>? parsedArguments, Dictionary<string, string>? parsedOptions)
        {
            if (!parsedDescriptors.Any())
            {
                throw new ErrorException(TerminalErrors.MissingCommand, "The command is missing in the command route.");
            }

            return Task.Run(() =>
            {
                // The last command in the route is the command that we are trying to execute.
                // Parse arguments and options
                CommandDescriptor executingCommandDescriptor = parsedDescriptors.Last();
                Arguments? arguments = ParseArguments(executingCommandDescriptor, parsedArguments);
                Options? options = ParseOptions(executingCommandDescriptor, parsedOptions);
                Command executingCommand = new(executingCommandDescriptor, options, arguments);

                // If we are here then the owners are valid and we can create the hierarchy
                Root? hierarchy = null;
                if (terminalOptions.Extractor.ParseHierarchy.GetValueOrDefault())
                {
                    Group? lastGroup = null;
                    SubCommand? lastSubCommand = null;
                    for (int idx = 0; idx < parsedDescriptors.Count; ++idx)
                    {
                        CommandDescriptor currentDescriptor = parsedDescriptors[idx];
                        Command currentCommand = new(currentDescriptor);
                        if (textHandler.TextEquals(currentDescriptor.Id, executingCommandDescriptor.Id))
                        {
                            currentCommand = executingCommand;
                        }

                        if (currentDescriptor.Type == CommandType.Root)
                        {
                            // There can be only root in the command route
                            if (hierarchy != null)
                            {
                                throw new ErrorException(TerminalErrors.InvalidCommand, "The command route contains multiple roots. root={0}", currentDescriptor.Id);
                            }

                            hierarchy = new Root(currentCommand);
                        }
                        else if (currentDescriptor.Type == CommandType.Group)
                        {
                            if (hierarchy == null)
                            {
                                throw new ErrorException(TerminalErrors.InvalidCommand, "The command group must be preceded by a root command. group={0}", currentDescriptor.Id);
                            }

                            Group group = new(currentCommand);
                            if (lastGroup == null)
                            {
                                hierarchy.ChildGroup = group;
                            }
                            else
                            {
                                lastGroup.ChildGroup = group;
                            }

                            lastGroup = group;
                        }
                        else if (currentDescriptor.Type == CommandType.SubCommand)
                        {
                            if (lastSubCommand != null)
                            {
                                throw new ErrorException(TerminalErrors.InvalidCommand, "The nested subcommands are not supported. command={0}", currentDescriptor.Id);
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
                        }
                        else
                        {
                            throw new ErrorException(TerminalErrors.InvalidRequest, "Invalid command descriptor type.");
                        }
                    }
                }

                return new ParsedCommand(commandRoute, executingCommand, hierarchy);
            });
        }

        private Options? ParseOptions(CommandDescriptor commandDescriptor, Dictionary<string, string>? parsedOptions)
        {
            if (parsedOptions == null || !parsedOptions.Any())
            {
                return null;
            }

            if (commandDescriptor.OptionDescriptors == null || !commandDescriptor.OptionDescriptors.Any())
            {
                throw new ErrorException(TerminalErrors.UnsupportedArgument, "The command does not support options. command={0}", commandDescriptor.Id);
            }

            List<Option> options = new();
            foreach (var optKvp in parsedOptions)
            {
                OptionDescriptor optionDescriptor = commandDescriptor.OptionDescriptors[optKvp.Key];
                options.Add(new Option(optionDescriptor, optKvp.Value));
            }
            return new Options(textHandler, options);
        }

        private Arguments? ParseArguments(CommandDescriptor commandDescriptor, List<string>? parsedArguments)
        {
            if (parsedArguments == null || !parsedArguments.Any())
            {
                return null;
            }

            if (commandDescriptor.ArgumentDescriptors == null || !commandDescriptor.ArgumentDescriptors.Any())
            {
                throw new ErrorException(TerminalErrors.UnsupportedArgument, "The command does not support arguments. command={0}", commandDescriptor.Id);
            }

            if (commandDescriptor.ArgumentDescriptors.Count < parsedArguments.Count)
            {
                throw new ErrorException(TerminalErrors.UnsupportedArgument, "The command does not support specified arguments. command={0} arguments={1}", commandDescriptor.Id, parsedArguments.JoinBySpace());
            }

            List<Argument> arguments = new();
            for (int idx = 0; idx < parsedArguments.Count; ++idx)
            {
                Argument argument = new(commandDescriptor.ArgumentDescriptors[idx], parsedArguments[idx]);
                arguments.Add(argument);
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