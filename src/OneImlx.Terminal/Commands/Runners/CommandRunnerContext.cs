/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Runtime;
using System;

namespace OneImlx.Terminal.Commands.Runners
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
        /// <exception cref="ArgumentNullException"></exception>
        public CommandRunnerContext(CommandHandlerContext handlerContext)
        {
            HandlerContext = handlerContext;
        }

        /// <summary>
        /// The command hander context.
        /// </summary>
        public CommandHandlerContext HandlerContext { get; }

        /// <summary>
        /// The command to run.
        /// </summary>
        public Command Command => HandlerContext.ParsedCommand.Command;

        /// <summary>
        /// The terminal start context.
        /// </summary>
        public TerminalStartContext StartContext => HandlerContext.RouterContext.TerminalContext.StartContext;

        /// <summary>
        /// The hierarchy of the command to run.
        /// </summary>
        public Root? Hierarchy => HandlerContext.ParsedCommand.Hierarchy;
    }
}