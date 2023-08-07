/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Configuration.Options;
using System.Linq;

namespace PerpetualIntelligence.Terminal.Extensions
{
    /// <summary>
    /// The <see cref="TerminalOptions"/> extensions methods.
    /// </summary>
    public static class TerminalOptionsExtensions
    {
        /// <summary>
        /// Returns a delimited command string for the specified raw messages.
        /// </summary>
        /// <param name="raw">The raw strings to be appended to the command string delimiter.</param>
        /// <param name="terminalOptions">The terminal options instance containing the command string delimiter.</param>
        /// <returns>The delimited command string.</returns>
        /// <remarks>
        /// The <see cref="DelimitedCommandString(TerminalOptions, string[])"/> method checks if each element in the raw array ends with the
        /// <see cref="RouterOptions.CommandStringDelimiter"/>. If it doesn't, the delimiter is appended to the string element. The formatted strings are then
        /// concatenated into a single command string.
        /// </remarks>
        public static string DelimitedCommandString(this TerminalOptions terminalOptions, params string[] raw)
        {
            var delimitedStrings = raw.Select(s => s.EndsWith(terminalOptions.Router.CommandStringDelimiter) ? s : string.Concat(s, terminalOptions.Router.CommandStringDelimiter));
            return string.Join(string.Empty, delimitedStrings);
        }
    }
}