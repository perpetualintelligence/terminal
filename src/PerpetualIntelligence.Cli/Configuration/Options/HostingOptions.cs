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

        /// <summary>
        /// Defines the hosting and routing error handling. Defaults to <c>default</c> error handling.
        /// </summary>
        public string ErrorHandling { get; set; } = "default";

        /// <summary>
        /// Defines the hosting and routing service implementation. Defaults to <c>default</c> service implementation.
        /// </summary>
        public string ServiceImplementation { get; set; } = "default";

        /// <summary>
        /// Defines the hosting and routing store. Defaults to <c>in_memory</c> store implementation.
        /// </summary>
        public string Store { get; set; } = "in_memory";

        /// <summary>
        /// Defines the hosting and routing service implementation. Defaults to <c>default</c> service implementation.
        /// </summary>
        public string UnicodeSupport { get; set; } = "default";
    }
}
