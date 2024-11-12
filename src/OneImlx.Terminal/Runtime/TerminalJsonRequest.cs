/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Text.Json.Serialization;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// A request class that encapsulates a command string that is enqueued and processed by the terminal server.
    /// </summary>
    public class TerminalJsonRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalJsonRequest"/> class.
        /// </summary>
        /// <param name="raw">The command or batch of commands.</param>
        [JsonConstructor]
        public TerminalJsonRequest(string raw)
        {
            Raw = raw;
        }

        /// <summary>
        /// The command string to be enqueued and processed by the terminal router.
        /// </summary>
        [JsonPropertyName("raw")]
        public string Raw { get; }
    }
}
