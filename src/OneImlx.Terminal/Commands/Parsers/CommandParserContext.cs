/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Commands.Parsers
{
    /// <summary>
    /// The command parser context.
    /// </summary>
    public sealed class CommandParserContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="request">The command request.</param>
        public CommandParserContext(TerminalRequest request)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }

        /// <summary>
        /// The command request.
        /// </summary>
        public TerminalRequest Request { get; set; }
    }
}