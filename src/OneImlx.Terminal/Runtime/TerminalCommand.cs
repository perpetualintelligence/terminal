/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Text.Json.Serialization;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Represents a single terminal command.
    /// </summary>
    public sealed class TerminalCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalCommand"/> class.
        /// </summary>
        /// <param name="id">The command id.</param>
        /// <param name="raw">The raw command.</param>
        [JsonConstructor]
        public TerminalCommand(string id, string raw)
        {
            Id = id;
            Raw = raw;
        }

        /// <summary>
        /// The unique command id.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; }

        /// <summary>
        /// The raw command.
        /// </summary>
        [JsonPropertyName("raw")]
        public string Raw { get; }

        /// <summary>
        /// Returns a string representation of the terminal command.
        /// </summary>
        /// <returns>A string containing the command id and command.</returns>
        public override string ToString()
        {
            return Raw;
        }
    }
}
