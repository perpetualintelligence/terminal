/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// An abstraction to resolve the license provider.
    /// </summary>
    public interface ILicenseProviderResolver
    {
        /// <summary>
        /// Resolves the provider and returns the provider identifier.
        /// </summary>
        /// <param name="resolveId">The resolve identifier.</param>
        /// <returns>The resolved identifier.</returns>
        public Task<string> ResolveAsync(string resolveId);
    }
}
