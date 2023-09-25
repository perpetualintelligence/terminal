/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Terminal.Commands;

namespace PerpetualIntelligence.Terminal.Hosting
{
    /// <summary>
    /// An abstraction of command builder.
    /// </summary>
    public interface ICommandBuilder
    {
        /// <summary>
        /// The service collection.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Build a new <see cref="CommandDescriptor"/> and add it to the service collection.
        /// </summary>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        ITerminalBuilder Add();
    }
}