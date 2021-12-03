/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

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
        public CommandRouterContext(string commandString)
        {
            CommandString = commandString;
        }

        /// <summary>
        /// The command string.
        /// </summary>
        public string CommandString { get; protected set; }
    }
}
