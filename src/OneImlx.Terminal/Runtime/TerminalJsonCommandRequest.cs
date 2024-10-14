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
    public class TerminalJsonCommandRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalJsonCommandRequest"/> class.
        /// </summary>
        /// <param name="commandString">The command string.</param>
        [JsonConstructor]
        public TerminalJsonCommandRequest(string commandString)
        {
            CommandString = commandString;
        }

        /// <summary>
        /// The command string to be enqueued and processed by the terminal router.
        /// </summary>
        [JsonPropertyName("command_string")]
        public string CommandString { get; }
    }
}
