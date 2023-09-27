/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Attributes;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Services;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PerpetualIntelligence.Terminal
{
    /// <summary>
    /// The helper methods.
    /// </summary>
    [WriteUnitTest]
    public static class TerminalHelper
    {
        /// <summary>
        /// Extracts the options from the raw option string.
        /// </summary>
        /// <param name="raw">The raw option string.</param>
        /// <param name="terminalOptions">The terminal options.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <returns></returns>
        public static OptionStrings ExtractOptionStrings(string raw, TerminalOptions terminalOptions, ITextHandler textHandler)
        {
            // Our search pattern is {separator}{prefix}, so we make sure we have just 1 separator at the beginning.
            string trimmedRaw = raw.TrimStart(terminalOptions.Extractor.Separator, textHandler.Comparison);
            trimmedRaw = string.Concat(terminalOptions.Extractor.Separator, trimmedRaw);

            // Define the separator split for both id and alias
            string optSplit = string.Concat(terminalOptions.Extractor.Separator, terminalOptions.Extractor.OptionPrefix);
            string optAliasSplit = string.Concat(terminalOptions.Extractor.Separator, terminalOptions.Extractor.OptionAliasPrefix);

            // Populate the range
            SortedDictionary<int, int> withInRanges = new();
            if (terminalOptions.Extractor.ValueDelimiter != null)
            {
                int withInIdx = 0;
                while (true)
                {
                    int startIdx = trimmedRaw.IndexOf(terminalOptions.Extractor.ValueDelimiter, withInIdx, textHandler.Comparison);
                    if (startIdx < 0)
                    {
                        break;
                    }

                    int endIdx = trimmedRaw.IndexOf(terminalOptions.Extractor.ValueDelimiter, startIdx + 1, textHandler.Comparison);
                    if (endIdx < 0)
                    {
                        throw new ErrorException(TerminalErrors.InvalidRequest, "The option value within end token is not specified.");
                    }

                    withInRanges.Add(startIdx, endIdx);

                    // Look for next with in range
                    withInIdx = endIdx + 1;
                }
            }

            int counter = 1;
            int currentOptionPos = 0;
            int currentIterationPos = 0;
            OptionStrings locations = new();
            while (true)
            {
                if (counter > 50)
                {
                    throw new ErrorException(TerminalErrors.InvalidConfiguration, $"Too many iteration while extracting options. max={50} current={counter}");
                }

                // Increment the nextPos to get the next option spit
                int nextPos = currentIterationPos + 1;

                // Iterate to next split positions.
                int nextOptIdPos = trimmedRaw.IndexOf(optSplit, nextPos, textHandler.Comparison);
                int nextOptAliasPos = trimmedRaw.IndexOf(optAliasSplit, nextPos, textHandler.Comparison);

                // No more matches so break. When the currentPos reaches the end then we have traversed the entire argString.
                int splitPos = 0;
                bool reachedEnd = false;
                if (nextOptIdPos == -1 && nextOptAliasPos == -1)
                {
                    // If we reach at the end then take the entire remaining string.
                    splitPos = trimmedRaw.Length;
                    reachedEnd = true;
                }
                else
                {
                    splitPos = InfraHelper.MinPositiveOrZero(nextOptIdPos, nextOptAliasPos);
                }

                // The currentIterationPos tracks each iteration so that we keep moving forward.
                currentIterationPos = splitPos;

                // Determine if split position is in between the with-in ranges
                if (!withInRanges.Any(e => splitPos > e.Key && splitPos < e.Value))
                {
                    // Get the arg substring and record its position and alias
                    // NOTE: This is the current pos and current alias not the next.
                    string kvp = trimmedRaw.Substring(currentOptionPos, splitPos - currentOptionPos);
                    bool isAlias = !kvp.StartsWith(optSplit, textHandler.Comparison);
                    locations.Add(new OptionString(kvp, isAlias, currentOptionPos));

                    // Update the currentPos only if we have processed the entire within range
                    currentOptionPos = currentIterationPos;
                }

                if (reachedEnd)
                {
                    break;
                }

                counter += 1;
            }

            return locations;
        }

        /// <summary>
        /// Determines if we are in a <c>dev-mode</c>. We assume <c>dev-mode</c> during debugging if the consumer deploys on-premise,
        /// use any source code editor or an IDE such as Visual Studio, Visual Studio Code, NotePad, Eclipse etc., use DEBUG or any other custom configuration.
        /// It is a violation of licensing terms to disable <c>dev-mode</c> with IL Weaving, Reflection or any other methods.
        /// </summary>
        /// <returns></returns>
        public static bool IsDevMode()
        {
            if (Debugger.IsAttached)
            {
                return true;
            }

#if RELEASE
            return false;
#else
            return true;
#endif
        }
    }
}