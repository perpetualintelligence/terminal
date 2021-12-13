/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;
using System.Threading;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The hosting configuration options.
    /// </summary>
    public class HostingOptions : OneImlxLoggingOptions
    {
        /// <summary>
        /// The command router timeout in milliseconds. Defaults to 10 seconds. Use <see cref="Timeout.Infinite"/> for
        /// no timeout.
        /// </summary>
        public int CommandRouterTimeout { get; set; } = 10000;
    }
}
