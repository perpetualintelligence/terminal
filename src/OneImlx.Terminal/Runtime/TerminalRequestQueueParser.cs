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
using OneImlx.Shared.Extensions;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Stores;

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
                Dictionary<string, string> parsedOptions = ExtractOptions(segmentsQueue);
                return new TerminalParsedRequest(tokens, parsedOptions);
            });
        }

        private Dictionary<string, string> ExtractOptions(Queue<string> segmentsQueue)
        {
            // This dictionary will hold the parsed options.
            TerminalOptions terminalOptions = this.terminalOptions.Value;
            Dictionary<string, string> parsedOptions = [];
            while (segmentsQueue.Count > 0)
            {
                // Always dequeue a segment because we're expecting it to be an option.
                string option = segmentsQueue.Dequeue();

                // Check whether we have an option value and if the option value is an option itself then the previous
                // option is a unary boolean option.
                if (segmentsQueue.Count > 0)
                {
                    string? optionValue = segmentsQueue.Peek();
                    if (!IsOption(optionValue, terminalOptions))
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
                        parsedOptions.Add(option, optionValue);
                        continue;
                    }
                }

                // If we are here that means the option is a unary boolean option.
                parsedOptions.Add(option, true.ToString());
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
            char us = terminalOptions.Parser.RuntimeSeparator;

            StringBuilder rawBuilder = new(raw, raw.Length);
            bool withinDelimiter = false;
            for (int idx = 0; idx < raw.Length; ++idx)
            {
                char currentChar = raw[idx];
                if (currentChar == valueDelimiter)
                {
                    withinDelimiter = !withinDelimiter;
                    continue;
                }

                if ((currentChar == separator || currentChar == valueSeparator) && !withinDelimiter)
                {
                    rawBuilder[idx] = us;
                }
            }

            // Split the raw command based on the UNIT SEPARATOR character.
            string[] segments = rawBuilder.ToString().Split([us], StringSplitOptions.RemoveEmptyEntries);

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
                if (IsOption(token, terminalOptions))
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

        private bool IsOption(string token, TerminalOptions terminalOptions)
        {
            return StartsWith(token, terminalOptions.Parser.OptionPrefix) ||
                   StartsWith(token, terminalOptions.Parser.OptionAliasPrefix);
        }

        private bool StartsWith(string value, string prefix)
        {
            return value.StartsWith(prefix, textHandler.Comparison);
        }

        private readonly ILogger<TerminalRequestQueueParser> logger;
        private readonly IOptions<TerminalOptions> terminalOptions;
        private readonly ITerminalTextHandler textHandler;
    }
}
