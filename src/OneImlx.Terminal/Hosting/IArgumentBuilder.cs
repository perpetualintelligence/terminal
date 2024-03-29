﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using OneImlx.Terminal.Commands;

namespace OneImlx.Terminal.Hosting
{
    /// <summary>
    /// An abstraction of argument builder.
    /// </summary>
    public interface IArgumentBuilder
    {
        /// <summary>
        /// The service collection.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Builds a new <see cref="ArgumentDescriptor"/> and add it to the service collection.
        /// </summary>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        ICommandBuilder Add();
    }
}