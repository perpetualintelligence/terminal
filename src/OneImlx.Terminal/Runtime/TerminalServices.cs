/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneImlx.Terminal.Configuration.Options;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Defines the common terminal services.
    /// </summary>
    public static class TerminalServices
    {
        /// <summary>
        /// Returns a delimited message for the specified raw messages.
        /// </summary>
        /// <param name="raw">The raw strings to be appended to the message delimiter.</param>
        /// <param name="terminalOptions">The terminal options instance containing the message delimiter.</param>
        /// <returns>The delimited message.</returns>
        /// <remarks>
        /// The <see cref="DelimitedMessage(TerminalOptions, string[])"/> method checks if each element in the raw array
        /// ends with the <see cref="RouterOptions.RemoteMessageDelimiter"/>. If it doesn't, the delimiter is appended
        /// to the string element. The formatted strings are then joined into a single message.
        /// </remarks>
        /// <seealso cref="DelimitedMessage(string, string[])"/>
        public static string DelimitedMessage(TerminalOptions terminalOptions, params string[] raw)
        {
            return DelimitedMessage(terminalOptions.Router.RemoteMessageDelimiter, raw);
        }

        /// <summary>
        /// Returns a delimited message for the specified raw messages.
        /// </summary>
        /// <param name="raw">The raw strings to be appended to the message delimiter.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns>The delimited message.</returns>
        /// <remarks>
        /// The <see cref="DelimitedMessage(string, string[])"/> method checks if each element in the raw array ends
        /// with the delimiter. If it doesn't, the delimiter is appended to the string element. The formatted strings
        /// are then joined into a single message.
        /// </remarks>
        /// <seealso cref="DelimitedMessage(TerminalOptions, string[])"/>
        public static string DelimitedMessage(string delimiter, params string[] raw)
        {
            var delimitedStrings = raw.Select(s => s.EndsWith(delimiter) ? s : string.Concat(s, delimiter));
            return string.Join(string.Empty, delimitedStrings);
        }
    }
}
