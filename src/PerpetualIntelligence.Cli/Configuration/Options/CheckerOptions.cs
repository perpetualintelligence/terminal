/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The <c>pi-cli</c> command and argument checker options.
    /// </summary>
    /// <remarks>The checker options are not filters. The command execution is blocked if any check fails.</remarks>
    public class CheckerOptions
    {
        /// <summary>
        /// Determines whether the checker allows a command to run with an obsolete argument.
        /// </summary>
        /// <remarks>
        /// The obsolete argument value check is done at runtime only if a user or an application attempts to run the
        /// command and passes an obsolete argument value. This option has no effect if the command supports an obsolete
        /// argument, but the user did not give its value through the command string.
        /// </remarks>
        public bool? AllowObsoleteArgument { get; set; }

        /// <summary>
        /// Determines whether the checker checks an argument value type. If this option is enabled, the checker will
        /// try to map an argument value to its corresponding .NET value type. If the mapping fails, the command will
        /// not run.
        /// </summary>
        /// <see cref="Commands.Mappers.IArgumentDataTypeMapper"/>
        public bool? StrictArgumentValueType { get; set; }
    }
}
