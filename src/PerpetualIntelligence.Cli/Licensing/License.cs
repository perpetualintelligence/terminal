/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Security.Claims;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// A <c>cli</c> license.
    /// </summary>
    public sealed class License : Shared.Infrastructure.License
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="licenseKey">The license key.</param>
        /// <param name="claimsPrincipal">The license claims principal.</param>
        public License(string licenseKey, ClaimsPrincipal claimsPrincipal) : base(claimsPrincipal)
        {
            LicenseKey = licenseKey;
        }

        /// <summary>
        /// The license key.
        /// </summary>
        public override string LicenseKey { get; }

        /// <summary>
        /// The maximum root commands. Defaults to <c>null</c> and it means no limit.
        /// </summary>
        public int? RootCommandLimit { get; internal set; }

        /// <summary>
        /// Disposes the license.
        /// </summary>
        public override void Dispose()
        {
        }
    }
}
