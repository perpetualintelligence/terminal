/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The help options.
    /// </summary>
    public class HelpOptions
    {
        /// <summary>
        /// Disables the help.
        /// </summary>
        public bool? Disabled { get; set; }

        /// <summary>
        /// The help argument identifier.
        /// </summary>
        /// <remarks>
        /// Unless <see cref="Disabled"/>, the help argument identifier is automatically added to the command arguments. The <see cref="HelpArgumentId"/> must be unique across all argument identifiers for all commands.
        /// </remarks>
        public string HelpArgumentId { get; set; } = "help";

        /// <summary>
        /// The help description.
        /// </summary>
        public string HelpArgumentDescription { get; set; } = "The command help.";

        /// <summary>
        /// The help argument alias.
        /// </summary>
        /// <remarks>
        /// Unless <see cref="Disabled"/>, the help argument alias is automatically added to the command arguments. The <see cref="HelpArgumentAlias"/> must be unique across all argument aliases for all commands.
        /// </remarks>
        public string HelpArgumentAlias { get; set; } = "H";
    }
}