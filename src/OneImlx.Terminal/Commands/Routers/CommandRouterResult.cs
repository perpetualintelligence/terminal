/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/
/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Commands.Routers
{
    /// <summary>
    /// The command router context.
    /// </summary>
    public sealed class CommandRouterResult
    {
        /// <summary>
        /// The command handler result.
        /// </summary>
        public CommandHandlerResult HandlerResult { get; }

        /// <summary>
        /// The command request.
        /// </summary>
        public TerminalRequest Request { get; }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="handlerResult">The handler result.</param>
        /// <param name="request">The command request.</param>
        public CommandRouterResult(CommandHandlerResult handlerResult, TerminalRequest request)
        {
            HandlerResult = handlerResult;
            Request = request;
        }
    }
}