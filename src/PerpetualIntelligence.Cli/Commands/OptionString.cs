/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// An immutable option string extracted from the <see cref="CommandString"/>.
    /// </summary>
    public sealed class OptionString
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="raw">The raw option string.</param>
        public OptionString(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new System.ArgumentException($"'{nameof(raw)}' cannot be null or whitespace.", nameof(raw));
            }

            Raw = raw;
        }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="raw">The raw option string.</param>
        /// <param name="aliasPrefix"><c>true</c> if the option string has an alias prefix, otherwise <c>false</c>.</param>
        /// <param name="position">The zero based position or index of the option string with in a command string.</param>
        public OptionString(string raw, bool aliasPrefix, int position)
        {
            Raw = raw;
            AliasPrefix = aliasPrefix;
            Position = position;
        }

        /// <summary>
        /// <c>true</c> if the option string has an alias prefix, otherwise <c>false</c>.
        /// </summary>
        public bool AliasPrefix { get; private set; }

        /// <summary>
        /// The zero based position or index of the option string with in a command string. ///
        /// </summary>
        public int Position { get; private set; }

        /// <summary>
        /// The option string.
        /// </summary>
        public string Raw { get; private set; }

        /// <summary>
        /// The string representation of <see cref="OptionString"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Raw;
        }
    }
}
