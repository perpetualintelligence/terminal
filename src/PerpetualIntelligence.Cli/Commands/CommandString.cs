/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Text.Json.Serialization;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// An immutable unicode textual form representing the command and its options or options that a user or an
    /// application wants to execute.
    /// </summary>
    public sealed class CommandString
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        [JsonConstructor]
        public CommandString(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new System.ArgumentException($"'{nameof(raw)}' cannot be null or whitespace.", nameof(raw));
            }

            Raw = raw;
        }

        /// <summary>
        /// The command string raw value.
        /// </summary>
        [JsonPropertyName("raw")]
        public string Raw { get; }
    }
}
