/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands;

namespace PerpetualIntelligence.Cli.Integration
{
    /// <summary>
    /// An abstraction of <c>pi-cli</c> argument builder.
    /// </summary>
    public interface IArgumentBuilder
    {
        /// <summary>
        /// The service collection.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Builds an <see cref="ArgumentDescriptor"/> and adds it to the service collection.
        /// </summary>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        ICommandBuilder Build();
    }
}
