/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Handlers;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    /// <summary>
    /// The command router context.
    /// </summary>
    public class CommandRouterResult
    {
        /// <summary>
        /// The command handler result.
        /// </summary>
        public CommandHandlerResult HandlerResult { get; set; }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="handlerResult"></param>
        public CommandRouterResult(CommandHandlerResult handlerResult)
        {
            HandlerResult = handlerResult;
        }
    }
}