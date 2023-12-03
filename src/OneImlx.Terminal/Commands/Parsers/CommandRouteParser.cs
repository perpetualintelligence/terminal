/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Shared.Extensions;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Parsers
{
    /// <summary>
    /// Represents a default command-line parser for processing terminal commands based on defined descriptors.
    /// </summary>
    /// <remarks>
    /// <para>The default command-line parser is designed for extensive command line parsing. Key features include:</para>
    ///
    /// <para><strong>Configurable Elements</strong>: Adapts to various command structures with configurable separators, delimiters, and option prefixes.</para>
    ///
    /// <para><strong>Segmentation and Queue</strong>: Segregates the command input string to separate commands/arguments from options. Segments are efficiently processed using a queue.</para>
    ///
    /// <para><strong>Command Parsing</strong>: Commands provide context for subsequent segments and must always precede arguments and options. The parser recognizes root commands, command groups, and subcommands.</para>
    ///
    /// <para><strong>Hierarchical Parsing (Optional)</strong>: Allows a hierarchical command structure with root, group, and subcommands. Note: Hierarchical parsing may affect the overall complexity of the parsing process.</para>
    ///
    /// <para><strong>Argument Parsing</strong>: Command arguments are processed before options. Any segment after the identified command, and that isn't recognized as an option or its alias, is treated as an argument. It's imperative that the provided arguments for a command don't exceed what its descriptor anticipates.</para>
    ///
    /// <para><strong>Option Parsing</strong>: Options follow arguments. The parser initially seeks the starting point of options in the input string. Once identified, each option, recognized by its prefix or alias prefix, is segmented using the option value separator. For an option to be valid, it must be defined in the command's descriptor.</para>
    ///
    /// <para><strong>Efficiency and Complexity</strong>: The parser's efficiency is derived from a combination of string operations and the sequential processing of segments. The initial identification of the starting point of options using `IndexOf` and subsequent `Substring` operations have a time complexity of O(n), where n is the length of the input string. The `Split` operations, both for commands/arguments and options, add another O(n). Processing segments through queue operations like Enqueue, Peek, and Dequeue have constant time complexities (O(1)) for each operation, but the cumulative time complexity across all segments remains O(n). Thus, considering all these operations, the overall complexity for the parsing process can be approximated as O(n). While the parser is designed to be comprehensive and handle a variety of command line structures efficiently, the actual performance can vary based on the intricacies of specific command structures and their length. It is always recommended to measure the parser's performance against real-world scenarios to assess its suitability for specific applications.</para>
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
    /// <exception cref="TerminalException">
    /// This exception is designed to capture a myriad of parsing issues such as unrecognized commands, unexpected number of arguments, or misidentified options.
    /// </exception>
    public class CommandRouteParser : ICommandRouteParser
    {
        private readonly ITextHandler textHandler;
        private readonly IImmutableCommandStore commandStore;
        private readonly TerminalOptions terminalOptions;
        private readonly ILogger<CommandRouteParser> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRouteParser"/> class.
        /// </summary>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="commandStore">The command store handler.</param>
        /// <param name="terminalOptions">The terminal configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CommandRouteParser(ITextHandler textHandler, IImmutableCommandStore commandStore, TerminalOptions terminalOptions, ILogger<CommandRouteParser> logger)
        {
            this.textHandler = textHandler;
            this.commandStore = commandStore;
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
        public async Task<ParsedCommand> ParseRouteAsync(CommandRoute commandRoute)
        {
            logger.LogDebug("Parse route. route={0} raw={1}", commandRoute.Id, commandRoute.Raw);

            // Parse the queue of segments from the raw command based of `separator` and `optionValueSeparator`
            Queue<ParsedSplit> segmentsQueue = ExtractQueue(commandRoute);

            // Handle the processing of commands and arguments
            (List<CommandDescriptor> parsedCommands, List<string> parsedArguments) = await ExtractCommandsAndArgumentsAsync(segmentsQueue);

            // Parse and process command options
            Dictionary<string, string> parsedOptions = ExtractOptions(segmentsQueue);

            // Log parsed details if debug level logging is enabled
            LogIfDebugLevelEnabled(parsedCommands, parsedArguments, parsedOptions);

            // Compile and return the parsed command details
            return ParseCommand(commandRoute, parsedCommands, parsedArguments, parsedOptions);
        }

        private Queue<ParsedSplit> ExtractQueue(CommandRoute commandRoute)
        {
            var queue = new Queue<ParsedSplit>();

            // This algorithm is designed to split a given string based on two token delimiters:
            // a primary separator and a value separator. The goal is to determine which of the
            // two tokens appears next in the string, allowing us to correctly split the string
            // into its logical segments.
            // 1. Start by initializing a `currentIndex` to zero, which indicates our current
            // position within the string.
            // 2. Using a while loop, iterate through the string until the entire length is processed.
            // 3. For each position denoted by `currentIndex`:
            //   a. Set the `nearestTokenIndex` to the end of the string. This value tracks the
            //      nearest occurrence of any token.
            //   b. Initialize `foundToken` to null. This holds the token (separator or valueSeparator)
            //      that is closest to the `currentIndex`.
            // 4. Loop through both tokens using a foreach loop:
            //   a. Find the first occurrence of each token from the current index.
            //   b. If a token occurrence is found and its position is closer than any previously
            //      found token's position, update `nearestTokenIndex` and `foundToken`.
            // 5. After the foreach loop:
            //   a. If `foundToken` is null (no more occurrences of either token), add the remainder
            //      of the string to the result and exit the loop.
            //   b. If a token is found, split the string at `nearestTokenIndex`, add the segment
            //      and its associated token to the result. Update `currentIndex` to continue processing.
            // The result is a list of segments split by the nearest occurring token, capturing the
            // context of each split.
            int currentIndex = 0;
            string raw = commandRoute.Raw;
            while (currentIndex < raw.Length)
            {
                int nearestTokenIndex = raw.Length;
                string? foundToken = null;
                foreach (var token in new[] { terminalOptions.Parser.Separator, terminalOptions.Parser.OptionValueSeparator })
                {
                    int index = raw.IndexOf(token, currentIndex);
                    if (index != -1 && index < nearestTokenIndex)
                    {
                        nearestTokenIndex = index;
                        foundToken = token;
                    }
                }

                if (foundToken == null)
                {
                    queue.Enqueue(new ParsedSplit(raw.Substring(currentIndex), null));
                    currentIndex = raw.Length;
                    continue;
                }

                string substring = raw.Substring(currentIndex, nearestTokenIndex - currentIndex);
                queue.Enqueue(new ParsedSplit(substring, foundToken));
                currentIndex = nearestTokenIndex + foundToken.Length;
            }

            return queue;
        }

        private Dictionary<string, string> ExtractOptions(Queue<ParsedSplit> segmentsQueue)
        {
            // This dictionary will hold the parsed options.
            Dictionary<string, string> parsedOptions = new();
            string valueDelimiter = terminalOptions.Parser.ValueDelimiter;
            string separator = terminalOptions.Parser.Separator;

            while (segmentsQueue.Count > 0)
            {
                // Always dequeue a segment because we're expecting it to be an option.
                ParsedSplit optionSplit = segmentsQueue.Dequeue();
                StringBuilder optionValueBuilder = new();

                // If we are not within a delimiter then we cannot have a separator.
                string option = optionSplit.Split;
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

                    nextSegment = segmentsQueue.Peek().Split;

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
                if (StartsWith(nextSegment, valueDelimiter))
                {
                    // If the next segment starts with a delimiter, then this segment represents a value that is enclosed
                    // within delimiters. We should process until we find the closing delimiter, capturing everything in-between.
                    bool foundClosingDelimiter = false;
                    while (segmentsQueue.Count > 0 && !foundClosingDelimiter)
                    {
                        ParsedSplit currentSegment = segmentsQueue.Dequeue();
                        optionValueBuilder.Append(currentSegment.Split);

                        // If the currentSegment contains just the delimiter both start and end will match and we will break out of the loop.
                        // This will happen when the option value for e.g. \"  value  "\
                        if (EndsWith(currentSegment.Split, valueDelimiter) && optionValueBuilder.Length > 1)
                        {
                            foundClosingDelimiter = true;
                        }

                        // Separator is not required if we found the closing delimiter.
                        bool requiresSeparator = segmentsQueue.Count > 0 && !foundClosingDelimiter;
                        if (requiresSeparator)
                        {
                            optionValueBuilder.Append(currentSegment.Token);
                        }
                    }

                    // Now, we use the flag to check if the closing delimiter was found.
                    if (!foundClosingDelimiter)
                    {
                        throw new ArgumentException($"Option '{option}' value does not have a closing delimiter.");
                    }

                    // Strip the delimiters if present.
                    string optionValueTemp = optionValueBuilder.ToString().TrimEnd(separator, textHandler.Comparison);
                    if (StartsWith(optionValueTemp, valueDelimiter))
                    {
                        optionValueTemp = RemovePrefix(optionValueTemp, valueDelimiter);
                    }
                    if (EndsWith(optionValueTemp, valueDelimiter))
                    {
                        optionValueTemp = RemoveSuffix(optionValueTemp, valueDelimiter);
                    }
                    parsedOptions[option] = optionValueTemp;
                }
                else
                {
                    // If the next segment is neither an option nor starts with a delimiter, then it should be treated as a
                    // non-delimited value for the current option. We process segments until we encounter the next option
                    // or until we exhaust all segments.
                    while (segmentsQueue.Count > 0 && !IsOption(segmentsQueue.Peek().Split))
                    {
                        if (optionValueBuilder.Length > 0)
                        {
                            optionValueBuilder.Append(separator); // Using space as a separator
                        }

                        optionValueBuilder.Append(segmentsQueue.Dequeue().Split);
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
        private async Task<(List<CommandDescriptor> ParsedDescriptors, List<string> ParsedArguments)> ExtractCommandsAndArgumentsAsync(Queue<ParsedSplit> segmentsQueue)
        {
            List<CommandDescriptor> parsedDescriptors = new();
            List<string> parsedArguments = new();
            string? potentialLastCommandId = null;
            string valueDelimiter = terminalOptions.Parser.ValueDelimiter;
            string separator = terminalOptions.Parser.Separator;

            while (segmentsQueue.Count > 0)
            {
                // Break loop if segment represents an option.
                ParsedSplit splitSegment = segmentsQueue.Peek();
                if (IsOption(splitSegment.Split))
                {
                    break;
                }
                segmentsQueue.Dequeue();

                // If we are not within a delimiter then we cannot have a separator.
                string segment = splitSegment.Split;
                if (segment.IsNullOrEmpty() || textHandler.TextEquals(segment, separator))
                {
                    continue;
                }

                if (await commandStore.TryFindByIdAsync(segment, out CommandDescriptor? currentDescriptor))
                {
                    if (currentDescriptor == null)
                    {
                        throw new TerminalException(TerminalErrors.ServerError, "Command found in the store but returned null descriptor.");
                    }

                    if (parsedArguments.Any())
                    {
                        potentialLastCommandId = parsedArguments.Last();
                    }

                    if (currentDescriptor != null)
                    {
                        if (potentialLastCommandId != null && (currentDescriptor.OwnerIds == null || !currentDescriptor.OwnerIds.Contains(potentialLastCommandId)))
                        {
                            throw new TerminalException(TerminalErrors.InvalidCommand, "The command owner is not valid. owner={0} command={1}.", potentialLastCommandId, currentDescriptor.Id);
                        }
                        else if (potentialLastCommandId == null && currentDescriptor.OwnerIds != null && currentDescriptor.OwnerIds.Any())
                        {
                            throw new TerminalException(TerminalErrors.MissingCommand, "The command owner is missing in the command route. owners={0} command={1}.", currentDescriptor.OwnerIds.JoinBySpace(), currentDescriptor.Id);
                        }

                        potentialLastCommandId = segment;
                        parsedDescriptors.Add(currentDescriptor);
                    }
                }
                else
                {
                    StringBuilder argumentValueBuilder = new(segment, segment.Length + 10);

                    if (StartsWith(segment, valueDelimiter))
                    {
                        while (segmentsQueue.Count > 0 && (!EndsWith(segment, valueDelimiter) || argumentValueBuilder.Length == 1))
                        {
                            argumentValueBuilder.Append(separator);
                            splitSegment = segmentsQueue.Dequeue();
                            segment = splitSegment.Split;
                            argumentValueBuilder.Append(segment);
                        }

                        if (!EndsWith(segment, valueDelimiter))
                        {
                            throw new TerminalException(TerminalErrors.InvalidCommand, "The argument value is missing the closing delimiter. argument={0}", argumentValueBuilder.ToString());
                        }
                    }

                    parsedArguments.Add(TrimValueDelimiter(argumentValueBuilder.ToString()));
                }
            }

            return (parsedDescriptors, parsedArguments);
        }

        private string TrimValueDelimiter(string value)
        {
            string trimmedValue = value;
            if (StartsWith(trimmedValue, terminalOptions.Parser.ValueDelimiter))
            {
                trimmedValue = RemovePrefix(trimmedValue, terminalOptions.Parser.ValueDelimiter);
            }

            if (EndsWith(trimmedValue, terminalOptions.Parser.ValueDelimiter))
            {
                trimmedValue = RemoveSuffix(trimmedValue, terminalOptions.Parser.ValueDelimiter);
            }

            return trimmedValue;
        }

        private void LogIfDebugLevelEnabled(List<CommandDescriptor> parsedDescriptors, IEnumerable<string> parsedArguments, Dictionary<string, string> parsedOptions)
        {
            // Early return to optimize performance.
            if (!logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            logger.LogDebug("Hierarchy={0}", parsedDescriptors.Select(e => e.Id).JoinByComma());
            logger.LogDebug("Command={0}", parsedDescriptors.LastOrDefault()?.Id);
            logger.LogDebug("Arguments={0}", parsedArguments.JoinByComma());
            logger.LogDebug("Options:");
            foreach (var opt in parsedOptions)
            {
                logger.LogDebug($"{opt.Key}=" + "{0}", opt.Value);
            }
        }

        private ParsedCommand ParseCommand(CommandRoute commandRoute, List<CommandDescriptor> parsedDescriptors, List<string>? parsedArguments, Dictionary<string, string>? parsedOptions)
        {
            if (!parsedDescriptors.Any())
            {
                throw new TerminalException(TerminalErrors.MissingCommand, "The command is missing in the command route.");
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
            if (!terminalOptions.Parser.ParseHierarchy.GetValueOrDefault())
            {
                return null;
            }

            Root? hierarchy = null;
            Group? lastGroup = null;
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
                                throw new TerminalException(TerminalErrors.InvalidCommand, "The command route contains multiple roots. root={0}", currentDescriptor.Id);
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
                                    throw new TerminalException(TerminalErrors.MissingCommand, "The command group is missing a root command. group={0}", currentCommand.Id);
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
                            break;
                        }

                    default:
                        {
                            throw new TerminalException(TerminalErrors.InvalidRequest, "The command descriptor type is not valid. type={0}", currentDescriptor.Type);
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
                throw new TerminalException(TerminalErrors.UnsupportedArgument, "The command does not support any options. command={0}", commandDescriptor.Id);
            }

            if (commandDescriptor.OptionDescriptors.Count < parsedOptions.Count)
            {
                throw new TerminalException(TerminalErrors.UnsupportedArgument, "The command does not support {0} options. command={1} options={2}", parsedOptions.Count, commandDescriptor.Id, parsedOptions.Keys.JoinByComma());
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
                    optionOrAliasKey = RemovePrefix(optKvp.Key, terminalOptions.Parser.OptionPrefix);
                }
                else
                {
                    optionOrAliasKey = RemovePrefix(optKvp.Key, terminalOptions.Parser.OptionAliasPrefix);
                }

                if (!commandDescriptor.OptionDescriptors.TryGetValue(optionOrAliasKey, out var optionDescriptor))
                {
                    throw new TerminalException(TerminalErrors.UnsupportedOption, "The command does not support an option or its alias. command={0} option={1}", commandDescriptor.Id, optionOrAliasKey);
                }

                if (isOption)
                {
                    // Validate if option matches expected id
                    if (!textHandler.TextEquals(optionDescriptor.Id, optionOrAliasKey))
                    {
                        throw new TerminalException(TerminalErrors.InvalidOption, "The option prefix is not valid for an alias. option={0}", optionOrAliasKey);
                    }
                }
                else
                {
                    // Validate if option matches expected alias
                    if (!textHandler.TextEquals(optionDescriptor.Alias, optionOrAliasKey))
                    {
                        throw new TerminalException(TerminalErrors.InvalidOption, "The alias prefix is not valid for an option. option={0}", optKvp.Key);
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
                throw new TerminalException(TerminalErrors.UnsupportedArgument, "The command does not support any arguments. command={0}", commandDescriptor.Id);
            }

            if (commandDescriptor.ArgumentDescriptors.Count < parsedArguments.Count)
            {
                throw new TerminalException(TerminalErrors.UnsupportedArgument, "The command does not support {0} arguments. command={1} arguments={2}", parsedArguments.Count, commandDescriptor.Id, parsedArguments.JoinByComma());
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
            return StartsWith(value, terminalOptions.Parser.OptionPrefix);
        }

        private bool IsAliasPrefix(string value)
        {
            return StartsWith(value, terminalOptions.Parser.OptionAliasPrefix);
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