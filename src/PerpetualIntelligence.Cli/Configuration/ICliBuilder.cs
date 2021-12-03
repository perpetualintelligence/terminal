/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;

namespace PerpetualIntelligence.OneImlx.Configuration
{
    /// <summary>
    /// An abstraction of <c>oneimlx</c> cli builder.
    /// </summary>
    public interface ICliBuilder
    {
        /// <summary>
        /// The service descriptors.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
