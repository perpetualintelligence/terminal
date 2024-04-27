/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Configuration.Options;
using System.Linq;

namespace OneImlx.Terminal.Extensions
{
    /// <summary>
    /// The <see cref="TerminalOptions"/> extensions methods.
    /// </summary>
    public static class TerminalOptionsExtensions
    {
        /// <summary>
        /// Returns a delimited message for the specified raw messages.
        /// </summary>
        /// <param name="raw">The raw strings to be appended to the message delimiter.</param>
        /// <param name="terminalOptions">The terminal options instance containing the message delimiter.</param>
        /// <returns>The delimited message.</returns>
        /// <remarks>
        /// The <see cref="DelimitedMessage(TerminalOptions, string[])"/> method checks if each element in the raw array ends with the
        /// <see cref="RouterOptions.RemoteMessageDelimiter"/>. If it doesn't, the delimiter is appended to the string element. The formatted strings are then
        /// joined into a single message.
        /// </remarks>
        public static string DelimitedMessage(this TerminalOptions terminalOptions, params string[] raw)
        {
            var delimitedStrings = raw.Select(s => s.EndsWith(terminalOptions.Router.RemoteMessageDelimiter) ? s : string.Concat(s, terminalOptions.Router.RemoteMessageDelimiter));
            return string.Join(string.Empty, delimitedStrings);
        }
    }
}