/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The command router options.
    /// </summary>
    public class RouterOptions
    {
        /// <summary>
        /// The command router timeout in milliseconds. The default value is 25 seconds. Use
        /// <see cref="Timeout.Infinite"/> for infinite timeout.
        /// </summary>
        /// <remarks>
        /// A command route starts at a request to execute the command and ends when the command run is complete or at an error.
        /// </remarks>
        public int Timeout { get; set; } = 25000;
    }
}
