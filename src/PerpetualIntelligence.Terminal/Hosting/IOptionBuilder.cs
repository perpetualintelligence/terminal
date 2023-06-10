/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Terminal.Commands;

namespace PerpetualIntelligence.Terminal.Hosting
{
    /// <summary>
    /// An abstraction of <c>pi-cli</c> option builder.
    /// </summary>
    public interface IOptionBuilder
    {
        /// <summary>
        /// The service collection.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Builds a new <see cref="OptionDescriptor"/> and add it to the service collection.
        /// </summary>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        ICommandBuilder Add();
    }
}
