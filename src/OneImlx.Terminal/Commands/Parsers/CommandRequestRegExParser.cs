/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Commands.Parsers
{
    /// <summary>
    /// </summary>
    public class CommandRequestRegExParser : ICommandRequestParser
    {
        /// <summary>
        /// </summary>
        public CommandRequestRegExParser(ILogger<CommandRequestRegExParser> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="TerminalException"></exception>
        public Task<ParsedCommand> ParseRequestAsync(TerminalRequest request)
        {
            logger.LogDebug("Parsing request with Regex. Raw input: {0}", request.Raw);

            string input = request.Raw;

            // Use virtual methods to get the regex patterns
            var commandRegex = GetCommandRegex();
            var argumentRegex = GetArgumentRegex();
            var optionRegex = GetOptionRegex();

            // Extract command
            var commandMatch = commandRegex.Match(input);
            if (!commandMatch.Success)
            {
                throw new TerminalException(TerminalErrors.MissingCommand, "No command found in input.");
            }

            string command = commandMatch.Groups[1].Value;
            logger.LogDebug("Command identified: {0}", command);

            // Extract options
            var options = new Dictionary<string, string>();
            foreach (Match match in optionRegex.Matches(input))
            {
                options[match.Groups[1].Value] = match.Groups[3].Value;
            }
            logger.LogDebug("Options parsed: {0}", string.Join(", ", options.Select(kvp => $"{kvp.Key}={kvp.Value}")));

            // Extract arguments
            var arguments = new List<string>();
            foreach (Match match in argumentRegex.Matches(input))
            {
                if (!options.ContainsKey(match.Groups[1].Value)) // Avoid treating options as arguments
                {
                    arguments.Add(match.Groups[1].Value);
                }
            }
            logger.LogDebug("Arguments parsed: {0}", string.Join(", ", arguments));

            // Build parsed command (mock method here for demonstration)
            return Task.FromResult(BuildParsedCommand(request, arguments, options));
        }

        /// <summary>
        /// Virtual method to get the regex for parsing arguments.
        /// </summary>
        /// <returns>The regex for parsing arguments.</returns>
        protected virtual Regex GetArgumentRegex()
        {
            return new Regex(@"(?:^|\s)([^\s-]+)(?=\s|$)", RegexOptions.Compiled);
        }

        /// <summary>
        /// Virtual method to get the regex for parsing commands.
        /// </summary>
        /// <returns>The regex for parsing commands.</returns>
        protected virtual Regex GetCommandRegex()
        {
            return new Regex(@"^(\w+)(?:\s|$)", RegexOptions.Compiled);
        }

        /// <summary>
        /// Virtual method to get the regex for parsing options.
        /// </summary>
        /// <returns>The regex for parsing options.</returns>
        protected virtual Regex GetOptionRegex()
        {
            return new Regex(@"-(\w+)=([""']?)(.+?)\2(?:\s|$)", RegexOptions.Compiled);
        }

        private ParsedCommand BuildParsedCommand(TerminalRequest request, List<string> arguments, Dictionary<string, string> options)
        {
            // Replace with actual logic to integrate parsed results into the framework.
            throw new NotImplementedException();
        }

        private readonly ILogger<CommandRequestRegExParser> logger;
    }
}
