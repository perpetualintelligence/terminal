/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Attributes;
using System;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The default command request context.
    /// </summary>
    public class CommandContext
    {
        /// <summary>
        /// The command string.
        /// </summary>
        /// <param name="commandString">The command string.</param>
        /// <param name="requestServices">The request services.</param>
        public CommandContext(string commandString, IServiceProvider requestServices)
        {
            CommandString = commandString;
            RequestServices = requestServices;
        }

        /// <summary>
        /// The command string.
        /// </summary>
        public string CommandString { get; protected set; }

        /// <summary>
        /// The request services.
        /// </summary>
        public IServiceProvider RequestServices { get; protected set; }

        /// <summary>
        /// The command runner result.
        /// </summary>
        [Refactor("This should be move to the result. The CommandRequestHandlerResult.ProcessResultAsync does not return anything. We need to change the IEndpointRequestResult design to return result.")]
        public CommandRunnerResult? RunnerResult { get; set; }
    }
}
