/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Extensions;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;

namespace OneImlx.Terminal.Commands.Parsers
{
    /// <summary>
    /// Represents a default queue based command-line parser for processing terminal commands based on defined descriptors.
    /// </summary>
    /// <remarks>
    /// <para>The default command-line parser is designed for extensive command line parsing. Key features include:</para>
    /// <para>
    /// <strong>Configurable Elements</strong>: Adapts to various command structures with configurable separators,
    /// delimiters, and option prefixes.
    /// </para>
    /// <para>
    /// <strong>Segmentation and Queue</strong>: Segregates the command input string to separate commands/arguments from
    /// options. Segments are efficiently processed using a queue.
    /// </para>
    /// <para>
    /// <strong>Command Parsing</strong>: Commands provide context for subsequent segments and must always precede
    /// arguments and options. The parser recognizes root commands, command groups, and subcommands.
    /// </para>
    /// <para>
    /// <strong>Hierarchical Parsing (Optional)</strong>: Allows a hierarchical command structure with root, group, and
    /// subcommands. Note: Hierarchical parsing may affect the overall complexity of the parsing process.
    /// </para>
    /// <para>
    /// <strong>Argument Parsing</strong>: Command arguments are processed before options. Any segment after the
    /// identified command, and that isn't recognized as an option or its alias, is treated as an argument. It's
    /// imperative that the provided arguments for a command don't exceed what its descriptor anticipates.
    /// </para>
    /// <para>
    /// <strong>Option Parsing</strong>: Options follow arguments. The parser initially seeks the starting point of
    /// options in the input string. Once identified, each option, recognized by its prefix or alias prefix, is
    /// segmented using the option value separator. For an option to be valid, it must be defined in the command's descriptor.
    /// </para>
    /// <para>
    /// <strong>Efficiency and Complexity</strong>: The parser's efficiency is derived from a combination of string
    /// operations and the sequential processing of segments. The initial identification of the starting point of
    /// options using `IndexOf` and subsequent `Substring` operations have a time complexity of O(n), where n is the
    /// length of the input string. The `Split` operations, both for commands/arguments and options, add another O(n).
    /// Processing segments through queue operations like Enqueue, Peek, and Dequeue have constant time complexities
    /// (O(1)) for each operation, but the cumulative time complexity across all segments remains O(n). Thus,
    /// considering all these operations, the overall complexity for the parsing process can be approximated as O(n).
    /// While the parser is designed to be comprehensive and handle a variety of command line structures efficiently,
    /// the actual performance can vary based on the intricacies of specific command structures and their length. It is
    /// always recommended to measure the parser's performance against real-world scenarios to assess its suitability
    /// for specific applications.
    /// </para>
    /// <para><strong>Potential Errors</strong>:
    /// <list type="bullet">
    /// <item>
    /// <description><c>invalid_command</c>: Occurs when multiple roots are detected in the command hierarchy.</description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>missing_command</c>: Raised when a command group is detected without a preceding root command.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>invalid_request</c>: Triggered by nested subcommands or an unrecognized command descriptor type.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>unsupported_argument</c>: Emitted when the command does not recognize the provided arguments or when the
    /// provided arguments surpass the number described in the command's descriptor.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>unsupported_option</c>: Resulted when an option or its alias isn't validated by the command's descriptor.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>invalid_option</c>: Happens when an option's ID is prefixed in the manner of an alias or the reverse.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Developer Note</strong>: While the default parser is optimized for a diverse set of command-line
    /// scenarios, if you possess a highly specialized or simplified parsing requirement, it might be beneficial to
    /// implement a custom parser. Nonetheless, it's advisable to thoroughly understand the capabilities and efficiency
    /// of this parser before transitioning to a custom implementation.
    /// </para>
    /// </remarks>
    /// <exception cref="TerminalException">
    /// This exception is designed to capture a myriad of parsing issues such as unrecognized commands, unexpected
    /// number of arguments, or misidentified options.
    /// </exception>
    public class TerminalRequestQueueParser : ITerminalRequestParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalRequestQueueParser"/> class.
        /// </summary>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="terminalOptions">The terminal configuration options.</param>
        /// <param name="logger">The logger.</param>
        public TerminalRequestQueueParser(
            ITerminalTextHandler textHandler,
            TerminalOptions terminalOptions,
            ILogger<TerminalRequestQueueParser> logger)
        {
            this.textHandler = textHandler;
            this.terminalOptions = terminalOptions;
            this.logger = logger;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<TerminalParsedRequest> ParseRequestAsync(TerminalRequest request)
        {
            return Task.Run(() =>
            {
                // Parse the queue of segments from the raw command based of `separator` and `optionValueSeparator`
                Queue<ParsedSplit> segmentsQueue = ExtractQueue(request);

                // Extract tokens and options from the segments queue
                IEnumerable<string> tokens = ExtractTokens(segmentsQueue);
                Dictionary<string, string> parsedOptions = ExtractOptions(segmentsQueue);
                return new TerminalParsedRequest(tokens, parsedOptions);
            });
        }

        private bool EndsWith(string value, string suffix)
        {
            return value.EndsWith(suffix, textHandler.Comparison);
        }

        private Dictionary<string, string> ExtractOptions(Queue<ParsedSplit> segmentsQueue)
        {
            // This dictionary will hold the parsed options.
            Dictionary<string, string> parsedOptions = [];
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

                // It's essential to skip over segments that are purely separators. Reasons for this are twofold:
                // 1. Ensures that our parsing logic does not mistakenly recognize separators as valid segments for
                // option values unless they are part of delimiters.
                // 2. Allows us to efficiently peek at the actual next segment of interest without being misled by
                // consecutive separators.
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

                // Peek at the next segment without removing it. This allows us to make a decision on how to proceed
                // without advancing the queue. E.g. whether the option value is delimited or not.
                if (StartsWith(nextSegment, valueDelimiter))
                {
                    // If the next segment starts with a delimiter, then this segment represents a value that is
                    // enclosed within delimiters. We should process until we find the closing delimiter, capturing
                    // everything in-between.
                    bool foundClosingDelimiter = false;
                    while (segmentsQueue.Count > 0 && !foundClosingDelimiter)
                    {
                        ParsedSplit currentSegment = segmentsQueue.Dequeue();
                        optionValueBuilder.Append(currentSegment.Split);

                        // If the currentSegment contains just the delimiter both start and end will match and we will
                        // break out of the loop. This will happen when the option value for e.g. \" value "\
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
                        throw new TerminalException(TerminalErrors.InvalidOption, "The option value is missing the closing delimiter. option={0}", option);
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
                    // If the next segment is neither an option nor starts with a delimiter, then it should be treated
                    // as a non-delimited value for the current option. We process segments until we encounter the next
                    // option or until we exhaust all segments.
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

        private Queue<ParsedSplit> ExtractQueue(TerminalRequest request)
        {
            var queue = new Queue<ParsedSplit>();

            // This algorithm is designed to split a given string based on two token delimiters: a primary separator and
            // a value separator. The goal is to determine which of the two tokens appears next in the string, allowing
            // us to correctly split the string into its logical segments.
            // 1. Start by initializing a `currentIndex` to zero, which indicates our current position within the string.
            // 2. Using a while loop, iterate through the string until the entire length is processed.
            // 3. For each position denoted by `currentIndex`: a. Set the `nearestTokenIndex` to the end of the string.
            // This value tracks the nearest occurrence of any token. b. Initialize `foundToken` to null. This holds the
            // token (separator or valueSeparator) that is closest to the `currentIndex`.
            // 4. Loop through both tokens using a foreach loop: a. Find the first occurrence of each token from the
            // current index. b. If a token occurrence is found and its position is closer than any previously found
            // token's position, update `nearestTokenIndex` and `foundToken`.
            // 5. After the foreach loop: a. If `foundToken` is null (no more occurrences of either token), add the
            // remainder of the string to the result and exit the loop. b. If a token is found, split the string at
            // `nearestTokenIndex`, add the segment and its associated token to the result. Update `currentIndex` to
            // continue processing. The result is a list of segments split by the nearest occurring token, capturing the
            // context of each split.
            int currentIndex = 0;
            string raw = request.Raw;
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

        private IEnumerable<string> ExtractTokens(Queue<ParsedSplit> segmentsQueue)
        {
            List<string> tokens = [];

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

                tokens.Add(segment);
            }

            return tokens;
        }

        private bool IsAliasPrefix(string value)
        {
            return StartsWith(value, terminalOptions.Parser.OptionAliasPrefix);
        }

        /// <summary>
        /// Checks if a given value represents an option.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the value represents an option; otherwise, false.</returns>
        /// <remarks>
        /// An option is recognized by specific prefixes that differentiate it from commands or arguments. This utility
        /// function checks for these prefixes (e.g., "--" or "-") and decides if the segment is an option or an alias
        /// for an option.
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

        private readonly ILogger<TerminalRequestQueueParser> logger;
        private readonly TerminalOptions terminalOptions;
        private readonly ITerminalTextHandler textHandler;
    }
}
