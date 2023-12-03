/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using OneImlx.Terminal.Commands.Handlers;
using System;

namespace OneImlx.Terminal.Hosting
{
    /// <summary>
    /// The default <see cref="ITerminalBuilder"/>.
    /// </summary>
    public sealed class TerminalBuilder : ITerminalBuilder
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="services">The global service collection.</param>
        /// <param name="textHandler">The global text handler.</param>
        /// <exception cref="ArgumentNullException">services</exception>
        public TerminalBuilder(IServiceCollection services, ITextHandler textHandler)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            TextHandler = textHandler ?? throw new ArgumentNullException(nameof(textHandler));
        }

        /// <summary>
        /// The global service collection.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// The text handler.
        /// </summary>
        public ITextHandler TextHandler { get; }
    }
}