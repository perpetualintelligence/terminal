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
    /// Represents a default command-line parser that understands and processes terminal commands based on pre-defined descriptors.
    /// </summary>
    /// <remarks>
    /// <para>The default command-line parser is designed for comprehensive command line parsing with a variety of use-cases in mind:</para>
    ///
    /// <para><strong>Configurable Elements</strong>: The parser accommodates configurable separators, delimiters, option prefixes, and option alias prefixes, making it adaptable for various command structures.</para>
    ///
    /// <para><strong>Segmentation and Queue</strong>: Before any form of parsing starts, the entire command input string is split into individual segments using the defined separator. These segments are then efficiently processed in order by using a queue data structure. The utilization of a queue ensures that every segment is processed once and only once, enhancing the efficiency of the algorithm.</para>
    ///
    /// <para><strong>Command Parsing</strong>: Commands are the first to be identified and processed from the segments. They provide context for subsequent segments and must always precede arguments and options. In a hierarchical structure, root commands must precede command groups and subcommands. The parser understands this structure and processes them in the correct order.</para>
    ///
    /// <para><strong>Hierarchical Parsing (Optional)</strong>: If enabled, commands can be structured hierarchically consisting of root, group, and subcommands. The hierarchy parsing requires a separate loop which affects the overall complexity of the parsing process. Each hierarchy should have a singular root. Commands are identified and processed before arguments or options.</para>
    ///
    /// <para><strong>Argument Parsing</strong>: Command arguments are processed before options. Any segment after the identified command, and that isn't recognized as an option or its alias, is treated as an argument. It's imperative that the provided arguments for a command don't exceed what its descriptor anticipates.</para>
    ///
    /// <para><strong>Option Parsing</strong>: Options follow arguments and are recognized by either their prefix or alias prefix. For an option to be valid, it must be defined in the command's descriptor.</para>
    ///
    /// <para><strong>Efficiency and Complexity</strong>: The essence of this parser's efficiency lies in the sequential processing of segments. When hierarchical parsing is disabled, the time complexity can be viewed as O(n), where n is the number of segments. With hierarchical parsing enabled, the complexity grows but remains efficient due to the singular processing of each segment. This ensures that the parser remains adept even as the complexity of the command grows. The algorithm has been designed to be comprehensive and cater to a broad spectrum of command line structures, making it suitable for a wide range of applications.</para>
    ///
    /// <para><strong>Potential Errors</strong>:
    /// <list type="bullet">
    /// <item><description><c>invalid_command</c>: Occurs when multiple roots are detected in the command hierarchy.</description></item>
    /// <item><description><c>missing_command</c>: Raised when a command group is detected without a preceding root command.</description></item>
    /// <item><description><c>invalid_request</c>: Triggered by nested subcommands or an unrecognized command descriptor type.</description></item>
    /// <item><description><c>unsupported_argument</c>: Emitted when the command does not recognize the provided arguments or when the provided arguments surpass the number described in the command's descriptor.</description></item>
    /// <item><description><c>unsupported_option</c>: Resulted when an option or its alias isn't validated by the command's descriptor.</description></item>
    /// <item><description><c>invalid_option</c>: Happens when an option's ID is prefixed in the manner of an alias or the reverse.</description></item>
    /// </list></para>
    ///
    /// <para><strong>Developer Note</strong>: While the default parser is optimized for a diverse set of command-line scenarios, if you possess a highly specialized or simplified parsing requirement, it might be beneficial to implement a custom parser. Nonetheless, it's advisable to thoroughly understand the capabilities and efficiency of this parser before transitioning to a custom implementation.</para>
    /// </remarks>
    /// <exception cref="ErrorException">
    /// This exception is designed to capture a myriad of parsing issues such as unrecognized commands, unexpected number of arguments, or misidentified options.
    /// </exception>
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
        /// Asynchronously parses a given input into structured command data.
        /// </summary>
        /// <remarks>
        /// This method provides a comprehensive solution for parsing terminal commands, offering developers the ability to extract relevant descriptors, options, and arguments from the provided input in an efficient manner.
        /// <para>Complexity Analysis:</para>
        /// The parsing process employs several phases, each with its own complexity:
        /// <list type="bullet">
        /// <item>
        /// <description><c>Initial String Split and Queue Construction:</c> The input string undergoes a preliminary split into segments. This operation's complexity is O(s), where s represents the length of the input string. Post splitting, segments are enqueued for orderly processing. The use of the Queue data structure ensures that segments are processed in a first-in-first-out manner, optimizing the parsing flow and ensuring order preservation.</description>
        /// </item>
        /// <item>
        /// <description><c>Command, Argument, and Option Extraction:</c> After the queue is populated, each segment is dequeued and evaluated sequentially to determine if it represents a command, option, or argument. The complexity of processing each segment is linear, O(n), where n is the number of segments.</description>
        /// </item>
        /// <item>
        /// <description><c>Hierarchy Parsing (Optional):</c> If hierarchy extraction is enabled, a command hierarchy is established. Its complexity, in the worst case, is linear with respect to the number of parsed command descriptors, O(m).</description>
        /// </item>
        /// </list>
        /// When taking into account all phases, the effective complexity of the algorithm is O(s + n), where the emphasis is on the fact that each segment is processed only once post splitting. This showcases the algorithm's efficiency, making it a robust choice for parsing terminal commands.
        /// However, developers should consider their specific parsing requirements. If you have simpler parsing needs, a custom implementation might allow for further optimizations. But for diverse and comprehensive parsing scenarios, this default implementation provides a well-rounded and efficient solution.
        /// </remarks>
        public async Task<ParsedCommand> ParseAsync(CommandRoute commandRoute)
        {
            // Segment the raw command into individual components
            HashSet<string> separatorSet = new() { terminalOptions.Extractor.Separator };
            Queue<string> segmentsQueue = new(commandRoute.Command.Raw.Split(separatorSet.ToArray(), StringSplitOptions.None));

            // Handle the processing of commands and arguments
            (List<CommandDescriptor> parsedCommands, List<string> parsedArguments) = await ExtractCommandsAndArgumentsAsync(segmentsQueue);

            // Extract and process command options
            Dictionary<string, string> parsedOptions = ExtractOptions(segmentsQueue);

            // Log parsed details if debug level logging is enabled
            LogIfDebugLevelEnabled(parsedCommands, parsedArguments, parsedOptions);

            // Compile and return the parsed command details
            return ParseCommand(commandRoute, parsedCommands, parsedArguments, parsedOptions);
        }

        private Dictionary<string, string> ExtractOptions(Queue<string> segmentsQueue)
        {
            // Handy configurable tokens.
            string delimiter = terminalOptions.Extractor.ValueDelimiter;
            string separator = terminalOptions.Extractor.Separator;

            // This dictionary will hold the parsed options.
            Dictionary<string, string> parsedOptions = new();

            while (segmentsQueue.Count > 0)
            {
                // Always dequeue a segment because we're expecting it to be an option.
                string option = segmentsQueue.Dequeue();
                StringBuilder optionValueBuilder = new();

                // If we are not within a delimiter then we cannot have a separator.
                if (option.IsNullOrEmpty() || textHandler.TextEquals(option, separator))
                {
                    continue;
                }

                // It's essential to skip over segments that are purely separators.
                // Reasons for this are twofold:
                // 1. Ensures that our parsing logic does not mistakenly recognize separators as valid segments for option values unless they are part of delimters.
                // 2. Allows us to efficiently peek at the actual next segment of interest without being misled by consecutive separators.
                string? nextSegment = null;
                while (true)
                {
                    if (segmentsQueue.Count == 0)
                    {
                        break; // Exit the loop if there are no more segments.
                    }

                    nextSegment = segmentsQueue.Peek();

                    // If nextSegment is purely the separator or empty, discard it and continue to the next iteration.
                    if (textHandler.TextEquals(nextSegment, separator) || string.IsNullOrEmpty(nextSegment))
                    {
                        segmentsQueue.Dequeue();
                        continue;
                    }

                    // Break when a segment that is neither null, empty, nor the separator is found.
                    break;
                }

                // If no more segments or the next is an option, current option's value is a default.
                if (nextSegment == null || IsOption(nextSegment))
                {
                    parsedOptions[option] = true.ToString();
                    continue;
                }

                // Peek at the next segment without removing it. This allows us to make a decision
                // on how to proceed without advancing the queue. E.g. whether the option value
                // is delimited or not.
                if (StartsWith(nextSegment, delimiter))
                {
                    // If the next segment starts with a delimiter, then this segment represents a value that is enclosed
                    // within delimiters. We should process until we find the closing delimiter, capturing everything in-between.
                    bool foundClosingDelimiter = false;
                    while (segmentsQueue.Count > 0 && !foundClosingDelimiter)
                    {
                        string currentSegment = segmentsQueue.Dequeue();
                        optionValueBuilder.Append(currentSegment);

                        // If the currentSegment contains just the delimiter both start and end will match and we will break out of the loop.
                        // This will happen when the option value for e.g. \"  value  "\
                        if (EndsWith(currentSegment, delimiter) && optionValueBuilder.Length > 1)
                        {
                            foundClosingDelimiter = true;
                        }

                        // Separator is not required if we found the closing delimiter.
                        bool requiresSeparator = segmentsQueue.Count > 0 && !foundClosingDelimiter;
                        if (requiresSeparator)
                        {
                            optionValueBuilder.Append(separator);
                        }
                    }

                    // Now, we use the flag to check if the closing delimiter was found.
                    if (!foundClosingDelimiter)
                    {
                        throw new ArgumentException($"Option '{option}' value does not have a closing delimiter.");
                    }

                    // Strip the delimiters if present.
                    string optionValueTemp = optionValueBuilder.ToString().TrimEnd(separator, textHandler.Comparison);
                    if (StartsWith(optionValueTemp, delimiter))
                    {
                        optionValueTemp = RemovePrefix(optionValueTemp, delimiter);
                    }
                    if (EndsWith(optionValueTemp, delimiter))
                    {
                        optionValueTemp = RemoveSuffix(optionValueTemp, delimiter);
                    }
                    parsedOptions[option] = optionValueTemp;
                }
                else
                {
                    // If the next segment is neither an option nor starts with a delimiter, then it should be treated as a
                    // non-delimited value for the current option. We process segments until we encounter the next option
                    // or until we exhaust all segments.
                    while (segmentsQueue.Count > 0 && !IsOption(segmentsQueue.Peek()))
                    {
                        if (optionValueBuilder.Length > 0)
                        {
                            optionValueBuilder.Append(separator); // Using space as a separator
                        }

                        optionValueBuilder.Append(segmentsQueue.Dequeue());
                    }

                    parsedOptions[option] = optionValueBuilder.ToString().TrimEnd(separator, textHandler.Comparison);
                }
            }

            return parsedOptions;
        }

        /// <summary>
        /// Extracts commands and arguments from a given queue of segments.
        /// </summary>
        /// <param name="segmentsQueue">A queue containing segments to be parsed.</param>
        /// <returns>A tuple containing a list of parsed command descriptors and a list of parsed arguments.</returns>
        /// <remarks>
        /// This method takes in a queue of string segments and progressively identifies if each segment is a command or an argument.
        /// The differentiation is crucial because commands and arguments are handled differently downstream.
        /// An argument, as recognized by this method, is a segment that doesn't correlate with any known command and follows an identified command.
        /// </remarks>
        private async Task<(List<CommandDescriptor> ParsedDescriptors, List<string> ParsedArguments)> ExtractCommandsAndArgumentsAsync(Queue<string> segmentsQueue)
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
                if (segment.IsNullOrEmpty() || textHandler.TextEquals(segment, separator))
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

                    if (currentDescriptor != null)
                    {
                        if (potentialLastCommandId != null && (currentDescriptor.OwnerIds == null || !currentDescriptor.OwnerIds.Contains(potentialLastCommandId)))
                        {
                            throw new ErrorException(TerminalErrors.InvalidCommand, "The command owner is not valid. owner={0} command={1}.", potentialLastCommandId, currentDescriptor.Id);
                        }
                        else if (potentialLastCommandId == null && currentDescriptor.OwnerIds != null && currentDescriptor.OwnerIds.Any())
                        {
                            throw new ErrorException(TerminalErrors.MissingCommand, "The command owner is missing in the command route. owners={0} command={1}.", currentDescriptor.OwnerIds.JoinBySpace(), currentDescriptor.Id);
                        }

                        potentialLastCommandId = segment;
                        parsedDescriptors.Add(currentDescriptor);
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
                ParseArguments(executingCommandDescriptor, parsedArguments),
                ParseOptions(executingCommandDescriptor, parsedOptions)
            );

            // Build the hierarchy and return the parsed command
            return new ParsedCommand(commandRoute, executingCommand, ParseHierarchy(parsedDescriptors, executingCommand));
        }

        /// <summary>
        /// Parses a hierarchy of commands and sub-commands based on a list of command descriptors.
        /// </summary>
        /// <param name="parsedDescriptors">A list of parsed command descriptors.</param>
        /// <param name="executingCommand">The command that is currently executing.</param>
        /// <returns>The root of the command hierarchy.</returns>
        /// <remarks>
        /// The method stitches together a command hierarchy. Starting with a root command, it systematically checks if a group or subcommand follows.
        /// It ensures the integrity of the hierarchy by validating there's only a single root, and that group and subcommands are in their correct positions.
        /// Misplacement or repetition results in exceptions, ensuring command structure correctness.
        /// </remarks>
        private Root? ParseHierarchy(List<CommandDescriptor> parsedDescriptors, Command executingCommand)
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

        /// <summary>
        /// Parses options provided in the command line arguments.
        /// </summary>
        /// <param name="commandDescriptor">The descriptor of the command being executed.</param>
        /// <param name="parsedOptions">A dictionary of parsed options.</param>
        /// <returns>A list of parsed options or null if no options were provided.</returns>
        /// <remarks>
        /// Options in a command line can be recognized either by their direct ID or an alias. This method takes in parsed options and tries to match them with the
        /// command descriptor's expected options. It ensures options are provided in a recognized format, whether by their full ID or their alias. Mismatches
        /// or unrecognized formats are caught and result in exceptions.
        /// </remarks>
        private Options? ParseOptions(CommandDescriptor commandDescriptor, Dictionary<string, string>? parsedOptions)
        {
            if (parsedOptions == null || parsedOptions.Count == 0)
            {
                return null;
            }

            if (commandDescriptor.OptionDescriptors == null || commandDescriptor.OptionDescriptors.Count == 0)
            {
                throw new ErrorException(TerminalErrors.UnsupportedArgument, "The command does not support any options. command={0}", commandDescriptor.Id);
            }

            if (commandDescriptor.OptionDescriptors.Count < parsedOptions.Count)
            {
                throw new ErrorException(TerminalErrors.UnsupportedArgument, "The command does not support {0} options. command={1} options={2}", parsedOptions.Count, commandDescriptor.Id, parsedOptions.Keys.JoinByComma());
            }

            // 1. An input can be either an option or an alias, but not both.
            // 2. If a segment is identified as an option, it must match the option ID.
            // 3. If identified as an alias, it must match the alias.
            List<Option> options = new(parsedOptions.Count);
            foreach (var optKvp in parsedOptions)
            {
                string optionOrAliasKey;
                bool isOption = IsOptionPrefix(optKvp.Key);

                if (isOption)
                {
                    optionOrAliasKey = RemovePrefix(optKvp.Key, terminalOptions.Extractor.OptionPrefix);
                }
                else
                {
                    optionOrAliasKey = RemovePrefix(optKvp.Key, terminalOptions.Extractor.OptionAliasPrefix);
                }

                if (!commandDescriptor.OptionDescriptors.TryGetValue(optionOrAliasKey, out var optionDescriptor))
                {
                    throw new ErrorException(TerminalErrors.UnsupportedOption, "The command does not support an option or its alias. command={0} option={1}", commandDescriptor.Id, optionOrAliasKey);
                }

                if (isOption)
                {
                    // Validate if option matches expected id
                    if (!textHandler.TextEquals(optionDescriptor.Id, optionOrAliasKey))
                    {
                        throw new ErrorException(TerminalErrors.InvalidOption, "The option prefix is not valid for an alias. option={0}", optionOrAliasKey);
                    }
                }
                else
                {
                    // Validate if option matches expected alias
                    if (!textHandler.TextEquals(optionDescriptor.Alias, optionOrAliasKey))
                    {
                        throw new ErrorException(TerminalErrors.InvalidOption, "The alias prefix is not valid for an option. option={0}", optKvp.Key);
                    }
                }

                options.Add(new Option(optionDescriptor, optKvp.Value));
            }

            return new Options(textHandler, options);
        }

        /// <summary>
        /// Parses arguments provided in the command line arguments.
        /// </summary>
        /// <param name="commandDescriptor">The descriptor of the command being executed.</param>
        /// <param name="parsedArguments">A list of parsed arguments.</param>
        /// <returns>A list of parsed arguments or null if no arguments were provided.</returns>
        /// <remarks>
        /// Parsing arguments involves matching each parsed segment with an expected argument from the command descriptor. This ensures that each command
        /// receives the arguments it expects, and no more. If the parsed segments contain more arguments than expected, or if they don't match the expected
        /// format, this method will throw an exception. This rigid structure ensures argument integrity for each command execution.
        /// </remarks>
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

        /// <summary>
        /// Checks if a given value represents an option.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the value represents an option; otherwise, false.</returns>
        /// <remarks>
        /// An option is recognized by specific prefixes that differentiate it from commands or arguments. This utility function checks for these prefixes
        /// (e.g., "--" or "-") and decides if the segment is an option or an alias for an option.
        /// </remarks>
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