﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Licensing;
using System;

namespace OneImlx.Terminal.Commands.Handlers
{
    /// <summary>
    /// The command handler context.
    /// </summary>
    public sealed class CommandHandlerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="routerContext">The command router context.</param>
        /// <param name="parsedCommand">The parsed command.</param>
        /// <param name="license">The extracted license.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandHandlerContext(CommandRouterContext routerContext, ParsedCommand parsedCommand, License license)
        {
            RouterContext = routerContext ?? throw new ArgumentNullException(nameof(routerContext));
            ParsedCommand = parsedCommand ?? throw new ArgumentNullException(nameof(parsedCommand));
            License = license ?? throw new ArgumentNullException(nameof(license));
        }

        /// <summary>
        /// The command router context.
        /// </summary>
        public CommandRouterContext RouterContext { get; }

        /// <summary>
        /// The parsed command to handle.
        /// </summary>
        public ParsedCommand ParsedCommand { get; }

        /// <summary>
        /// The extracted licenses.
        /// </summary>
        public License License { get; }
    }
}