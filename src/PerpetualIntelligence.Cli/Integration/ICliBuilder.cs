/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;

namespace PerpetualIntelligence.Cli.Integration
{
    /// <summary>
    /// An abstraction of Perpetual Intelligence <c>cli</c> builder.
    /// </summary>
    public interface ICliBuilder
    {
        /// <summary>
        /// The service collection.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
