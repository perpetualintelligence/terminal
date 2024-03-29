﻿/*
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
        /// The command route.
        /// </summary>
        public CommandRoute Route { get; }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="handlerResult">The handler result.</param>
        /// <param name="route">The command route.</param>
        public CommandRouterResult(CommandHandlerResult handlerResult, CommandRoute route)
        {
            HandlerResult = handlerResult;
            Route = route;
        }
    }
}