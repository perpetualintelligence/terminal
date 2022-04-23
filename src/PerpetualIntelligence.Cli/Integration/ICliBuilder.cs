/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;

namespace PerpetualIntelligence.Cli.Integration
{
    /// <summary>
    /// An abstraction of <c>pi-cli</c> service builder.
    /// </summary>
    public interface ICliBuilder
    {
        /// <summary>
        /// The service collection.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
