/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Configuration.Options;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRequestParser"/> that uses a queue to parse the terminal request.
    /// </summary>
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
            IOptions<TerminalOptions> terminalOptions,
            ILogger<TerminalRequestQueueParser> logger)
        {
            this.textHandler = textHandler;
            this.terminalOptions = terminalOptions;
            this.logger = logger;
        }

        /// <summary>
        /// Parses the terminal request asynchronously.
        /// </summary>
        /// <param name="request">The terminal request to parse.</param>
        /// <returns>
        /// A task that represents the asynchronous parse operation. The task result contains the parsed request.
        /// </returns>
        public Task<TerminalParsedRequest> ParseRequestAsync(TerminalRequest request)
        {
            return Task.Run(() =>
            {
                // Parse the queue of segments from the raw command based on `separator` and `optionValueSeparator`
                Queue<string> segmentsQueue = ExtractQueue(request);

                // Extract tokens and options from the segments queue
                IEnumerable<string> tokens = ExtractTokens(segmentsQueue);
                Dictionary<string, ValueTuple<string, bool>> parsedOptions = ExtractOptions(segmentsQueue);
                return new TerminalParsedRequest(tokens, parsedOptions);
            });
        }

        private Dictionary<string, ValueTuple<string, bool>> ExtractOptions(Queue<string> segmentsQueue)
        {
            // This dictionary will hold the parsed options.
            TerminalOptions terminalOptions = this.terminalOptions.Value;
            Dictionary<string, ValueTuple<string, bool>> parsedOptions = [];
            while (segmentsQueue.Count > 0)
            {
                // Always dequeue a segment because we're expecting it to be an option.
                string option = segmentsQueue.Dequeue();
                _ = TerminalServices.IsOption(option, terminalOptions.Parser.OptionPrefix, out bool isAlias);

                // If the option alias is disabled then we do not support short, long and hybrid options.
                // - With alias: -a, --all, --long-option,
                // - Without alias: -a, -all, -longoption
                if (terminalOptions.Parser.DisableOptionAlias)
                {
                    // Override the alias flag if the option alias is disabled.
                    isAlias = false;
                    option = option.Substring(1);
                }
                else
                {
                    // Remove the first character if it is an alias prefix otherwise remove 2 characters if it is an
                    // option prefix.
                    option = isAlias ? option.Substring(1) : option.Substring(2);
                }

                // Check whether we have an option value and if the option value is an option itself then the previous
                // option is a unary boolean option.
                if (segmentsQueue.Count > 0)
                {
                    string optionValue = segmentsQueue.Peek();
                    if (!TerminalServices.IsOption(optionValue, terminalOptions.Parser.OptionPrefix, out _))
                    {
                        // Ensure token ends with a delimiter
                        if (optionValue.First() == terminalOptions.Parser.ValueDelimiter)
                        {
                            if (optionValue.Last() != terminalOptions.Parser.ValueDelimiter)
                            {
                                throw new TerminalException(TerminalErrors.InvalidOption, "The option value is missing the closing delimiter. option={0}", option);
                            }

                            // Remove the delimiter and return the raw value.
                            optionValue = optionValue.Trim(terminalOptions.Parser.ValueDelimiter);
                        }

                        // The option value is processed to remove it from the queue, so we can process the next option.
                        segmentsQueue.Dequeue();
                        parsedOptions.Add(option, new(optionValue, isAlias));
                        continue;
                    }
                }

                // If we are here that means the option is a unary boolean option.
                parsedOptions.Add(option, new(true.ToString(), isAlias));
            }

            return parsedOptions;
        }

        private Queue<string> ExtractQueue(TerminalRequest request)
        {
            TerminalOptions terminalOptions = this.terminalOptions.Value;
            Queue<string> segmentQueue = new();

            // Parse the raw and replace all occurrences of separator that are not within delimiters with a UNIT
            // SEPARATOR character.
            string raw = request.Raw;
            char valueDelimiter = terminalOptions.Parser.ValueDelimiter;
            char separator = terminalOptions.Parser.Separator;
            char valueSeparator = terminalOptions.Parser.OptionValueSeparator;
            char runtimeSeparator = terminalOptions.Parser.RuntimeSeparator;

            StringBuilder rawBuilder = new(raw, raw.Length);
            bool withinDelimiter = false;
            for (int idx = 0; idx < raw.Length; ++idx)
            {
                char currentChar = raw[idx];

                // If we are within a value delimiter then no parsing logic is applied. The value delimiter are for
                // arguments and options values. So a value delimiter will always have a preceding separator.
                if (currentChar == valueDelimiter)
                {
                    withinDelimiter = !withinDelimiter;
                    continue;
                }

                // Replace the separator with the runtime separator if it is not within a delimiter.
                if ((currentChar == separator || currentChar == valueSeparator) && !withinDelimiter)
                {
                    rawBuilder[idx] = runtimeSeparator;
                }
            }

            // Split the raw command based on the runtime separator character.
            string[] segments = rawBuilder.ToString().Split([runtimeSeparator], StringSplitOptions.RemoveEmptyEntries);

            // Populate queue with the split segments.
            foreach (string segment in segments)
            {
                segmentQueue.Enqueue(segment);
            }

            return segmentQueue;
        }

        private IEnumerable<string> ExtractTokens(Queue<string> segmentQueue)
        {
            TerminalOptions terminalOptions = this.terminalOptions.Value;
            List<string> tokens = [];
            while (segmentQueue.Count > 0)
            {
                // Break loop if segment represents an option.
                string token = segmentQueue.Peek();
                if (TerminalServices.IsOption(token, terminalOptions.Parser.OptionPrefix, out _))
                {
                    break;
                }

                // Ensure token ends with a delimiter
                if (token.First() == terminalOptions.Parser.ValueDelimiter)
                {
                    if (token.Last() != terminalOptions.Parser.ValueDelimiter)
                    {
                        throw new TerminalException(TerminalErrors.InvalidArgument, "The argument value is missing the closing delimiter. argument={0}", token);
                    }

                    token = token.Trim(terminalOptions.Parser.ValueDelimiter);
                }

                // Not an option so now dequeue and process the token.
                segmentQueue.Dequeue();
                tokens.Add(token);
            }
            return tokens;
        }

        private readonly ILogger<TerminalRequestQueueParser> logger;
        private readonly IOptions<TerminalOptions> terminalOptions;
        private readonly ITerminalTextHandler textHandler;
    }
}
