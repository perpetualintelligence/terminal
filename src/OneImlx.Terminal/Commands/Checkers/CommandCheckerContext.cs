/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Handlers;
using System;

namespace OneImlx.Terminal.Commands.Checkers
{
    /// <summary>
    /// The command checker context.
    /// </summary>
    public class CommandCheckerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="handlerContext">The command handler context.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandCheckerContext(CommandHandlerContext handlerContext)
        {
            HandlerContext = handlerContext ?? throw new ArgumentNullException(nameof(handlerContext));
        }

        /// <summary>
        /// The command handler context.
        /// </summary>
        public CommandHandlerContext HandlerContext { get; }
    }
}