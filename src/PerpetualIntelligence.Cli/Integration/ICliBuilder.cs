/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands.Handlers;

namespace PerpetualIntelligence.Cli.Integration
{
    /// <summary>
    /// An abstraction of <c>pi-cli</c> service builder.
    /// </summary>
    public interface ICliBuilder
    {
        /// <summary>
        /// The global service collection.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
