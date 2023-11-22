﻿/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using System;

namespace PerpetualIntelligence.Terminal.Hosting
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
        /// <exception cref="ArgumentNullException">services</exception>
        public TerminalBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// The global service collection.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}