/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    /// <summary>
    /// The <c>pi-cli</c> command and option checker options.
    /// </summary>
    /// <remarks>The checker options are not filters. The command execution is blocked if any check fails.</remarks>
    public class CheckerOptions
    {
        /// <summary>
        /// Determines whether the checker allows a command to run with an obsolete option.
        /// </summary>
        /// <remarks>
        /// The obsolete option value check is done at runtime only if a user or an application attempts to run the
        /// command and passes an obsolete option value. This option has no effect if the command supports an obsolete
        /// option, but the user did not give its value through the command string.
        /// </remarks>
        public bool? AllowObsoleteOption { get; set; }

        /// <summary>
        /// Determines whether the checker checks an option value type. If this option is enabled, the checker will
        /// try to map an option value to its corresponding .NET value type. If the mapping fails, the command will
        /// not run.
        /// </summary>
        /// <see cref="Commands.Mappers.IOptionDataTypeMapper"/>
        public bool? StrictOptionValueType { get; set; }
    }
}
