/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Licensing.Models;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// A <c>cli</c> license.
    /// </summary>
    public sealed class License : System.ComponentModel.License
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="licenseKey">The license key.</param>
        /// <param name="claims">The license claims.</param>
        /// <param name="limits">The license limits.</param>
        public License(string licenseKey, LicenseClaimsModel claims, LicenseLimits limits)
        {
            this.licenseKey = licenseKey;
            Limits = limits;
            Claims = claims;
        }

        /// <summary>
        /// The licensing claims.
        /// </summary>
        public LicenseClaimsModel Claims { get; }

        /// <summary>
        /// The license key.
        /// </summary>
        public override string LicenseKey => licenseKey;

        /// <summary>
        /// The licensing limits.
        /// </summary>
        public LicenseLimits Limits { get; }

        /// <summary>
        /// Disposes the license.
        /// </summary>
        public override void Dispose()
        {
        }

        private readonly string licenseKey;
    }
}
