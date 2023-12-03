/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    /// <summary>
    /// The command, argument, and option checker configuration options.
    /// </summary>
    /// <remarks>The checker options are not filters. The command execution is blocked if any check fails.</remarks>
    public sealed class CheckerOptions
    {
        /// <summary>
        /// Determines whether the checker allows a command to run with an obsolete argument or an option.
        /// </summary>
        /// <remarks>
        /// The obsolete check is done at runtime only if a user or an application attempts to run the
        /// command and passes an obsolete argument or an option.
        /// </remarks>
        public bool? AllowObsolete { get; set; }

        /// <summary>
        /// Determines whether the checker checks an option value type. If this option is enabled, the checker will
        /// try to map an option value to its corresponding .NET value type. If the mapping fails, the command will
        /// not run.
        /// </summary>
        /// <see cref="Commands.Mappers.IDataTypeMapper{TValue}"/>
        public bool? StrictValueType { get; set; }
    }
}