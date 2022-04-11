/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Licensing;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// The default <see cref="ILicenseProviderResolver"/>.
    /// </summary>
    public class LicenseProviderResolver : ILicenseProviderResolver
    {
        /// <inheritdoc/>
        public Task<string> ResolveAsync(string resolveId)
        {
            if (SaaSProviders.IsValid(resolveId))
            {
                if (resolveId == SaaSProviders.PerpetualIntelligence)
                {
                    return Task.FromResult("b97520f6-250f-4940-961d-76f22245a47e");
                }
            }

            return Task.FromResult(resolveId);
        }
    }
}
