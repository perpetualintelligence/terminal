/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading;

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    /// <summary>
    /// The command router options.
    /// </summary>
    public class RouterOptions
    {
        /// <summary>
        /// The terminal caret to show in the console. The default value is <c>></c>.
        /// </summary>
        public string Caret { get; set; } = ">";

        /// <summary>
        /// The command router timeout in milliseconds. The default value is 25 seconds. Use
        /// <see cref="Timeout.Infinite"/> for infinite timeout.
        /// </summary>
        /// <remarks>
        /// A command route starts at a request to execute the command and ends when the command run is complete or at an error.
        /// </remarks>
        public int Timeout { get; set; } = 25000;

        /// <summary>
        /// Allows threads or tasks to sync during command routing. The default value is 100 milliseconds.
        /// </summary>
        public int? SyncDelay { get; set; } = 100;

        /// <summary>
        /// The maximum number of active remote client connections the router can accept. The default value is 5.
        /// </summary>
        public int RemoteMaxClients { get; set; } = 5;

        /// <summary>
        /// The read timeout from a remote source such as a network stream. The default value is 5 seconds.
        /// </summary>
        public int RemoteReadTimeout { get; set; } = 5000;

        /// <summary>
        /// The maximum length of a command string. The default value is 1024.
        /// </summary>
        public int MaxCommandStringLength { get; set; } = 1024;
    }
}