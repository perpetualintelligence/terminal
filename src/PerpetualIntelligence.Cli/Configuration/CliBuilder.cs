/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using System;

namespace PerpetualIntelligence.OneImlx.Configuration
{
    /// <summary>
    /// The <c>oneimlx</c> cli builder.
    /// </summary>
    public class CliBuilder : ICliBuilder
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <exception cref="System.ArgumentNullException">services</exception>
        public CliBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// The services.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
