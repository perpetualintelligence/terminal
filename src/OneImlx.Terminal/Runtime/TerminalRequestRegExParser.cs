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
    public class TerminalRequestRegExParser : ITerminalRequestParser
    {
        /// <summary>
        /// </summary>
        public TerminalRequestRegExParser(ILogger<TerminalRequestRegExParser> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<ParsedRequest> ParseOutputAsync(TerminalRequest request)
        {
            throw new NotImplementedException();
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

        private readonly ILogger<TerminalRequestRegExParser> logger;
    }
}
