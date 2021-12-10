/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System.Threading;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The <c>cli</c> generic command router context.
    /// </summary>
    public class CommandRouterContext
    {
        /// <summary>
        /// The command string.
        /// </summary>
        /// <param name="commandString">The command string.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public CommandRouterContext(string commandString, CancellationToken? cancellationToken = null)
        {
            CommandString = commandString;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// The cancellation token.
        /// </summary>
        public CancellationToken? CancellationToken { get; protected set; }

        /// <summary>
        /// The command string.
        /// </summary>
        public string CommandString { get; protected set; }
    }
}
