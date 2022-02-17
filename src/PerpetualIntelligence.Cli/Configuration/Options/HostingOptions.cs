/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The hosting configuration options.
    /// </summary>
    public class HostingOptions : Shared.Infrastructure.LoggingOptions
    {
        /// <summary>
        /// The command router timeout in milliseconds. Defaults to 10 seconds. Use <see cref="Timeout.Infinite"/> for
        /// no timeout.
        /// </summary>
        public int CommandRouterTimeout { get; set; } = 10000;
    }
}
