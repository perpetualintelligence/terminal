/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Text.Json.Serialization;

namespace PerpetualIntelligence.Terminal.Commands
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
                throw new System.ArgumentNullException(nameof(raw), $"'{nameof(raw)}' cannot be null or whitespace.");
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
