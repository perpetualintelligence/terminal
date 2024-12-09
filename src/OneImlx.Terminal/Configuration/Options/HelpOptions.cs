/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Configuration.Options
{
    /// <summary>
    /// The help options.
    /// </summary>
    public sealed class HelpOptions
    {
        /// <summary>
        /// Disables the help.
        /// </summary>
        public bool? Disabled { get; set; }

        /// <summary>
        /// The help option alias.
        /// </summary>
        /// <remarks>
        /// Unless <see cref="Disabled"/>, the help option alias is automatically added to the command options. The
        /// <see cref="OptionAlias"/> must be unique across all option aliases for all commands.
        /// </remarks>
        public string OptionAlias { get; set; } = "h";

        /// <summary>
        /// The help description.
        /// </summary>
        public string OptionDescription { get; set; } = "The command help.";

        /// <summary>
        /// The help option identifier.
        /// </summary>
        /// <remarks>
        /// Unless <see cref="Disabled"/>, the help option identifier is automatically added to the command options. The
        /// <see cref="OptionId"/> must be unique across all option identifiers for all commands.
        /// </remarks>
        public string OptionId { get; set; } = "help";
    }
}
