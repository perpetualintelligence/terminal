/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// An immutable <c>pi-cli</c> command string.
    /// </summary>
    public sealed class CommandString
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public CommandString(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new System.ArgumentException($"'{nameof(raw)}' cannot be null or whitespace.", nameof(raw));
            }

            Raw = raw;
        }

        /// <summary>
        /// The command raw string value.
        /// </summary>
        public string Raw { get; }
    }
}
