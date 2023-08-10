/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Handlers;
using System;
using System.Collections.Generic;

namespace PerpetualIntelligence.Terminal.Commands.Runners
{
    /// <summary>
    /// The command runner context.
    /// </summary>
    public sealed class CommandRunnerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="handlerContext">The command hander context.</param>
        /// <param name="custom">The custom data for the command to run.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandRunnerContext(CommandHandlerContext handlerContext, Dictionary<string, object>? custom = null)
        {
            HandlerContext = handlerContext;
            Custom = custom;
        }

        /// <summary>
        /// The command hander context.
        /// </summary>
        public CommandHandlerContext HandlerContext { get; }

        /// <summary>
        /// The custom data for the command.
        /// </summary>
        public Dictionary<string, object>? Custom { get; }
    }
}