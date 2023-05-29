/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands.Handlers;

namespace PerpetualIntelligence.Cli.Hosting
{
    /// <summary>
    /// An abstraction of <c>pi-cli</c> service builder.
    /// </summary>
    public interface ITerminalBuilder
    {
        /// <summary>
        /// The global service collection.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
