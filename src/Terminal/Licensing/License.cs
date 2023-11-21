/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Licensing;

namespace PerpetualIntelligence.Terminal.Licensing
{
    /// <summary>
    /// A terminal framework license.
    /// </summary>
    public sealed class License : System.ComponentModel.License
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="providerTenantId">The license provider tenant id.</param>
        /// <param name="handler">The license handler.</param>
        /// <param name="plan">The license plan.</param>
        /// <param name="usage">The license usage.</param>
        /// <param name="licenseKeySource"></param>
        /// <param name="licenseKey">The license key.</param>
        /// <param name="claims">The license claims.</param>
        /// <param name="limits">The license limits.</param>
        /// <param name="price">The license price.</param>
        public License(string providerTenantId, string handler, string plan, string usage, string licenseKeySource, string licenseKey, LicenseClaimsModel claims, LicenseLimits limits, LicensePrice price)
        {
            ProviderId = providerTenantId;
            Handler = handler;
            Plan = plan;
            Usage = usage;
            LicenseKeySource = licenseKeySource;
            this.licenseKey = licenseKey;
            Limits = limits;
            Price = price;
            Claims = claims;
        }

        /// <summary>
        /// The license claims.
        /// </summary>
        public LicenseClaimsModel Claims { get; }

        /// <summary>
        /// The license handler.
        /// </summary>
        public string Handler { get; }

        /// <summary>
        /// The license key.
        /// </summary>
        public override string LicenseKey => licenseKey;

        /// <summary>
        /// The license key source.
        /// </summary>
        public string LicenseKeySource { get; }

        /// <summary>
        /// The license limits.
        /// </summary>
        public LicenseLimits Limits { get; }

        /// <summary>
        /// The license plan.
        /// </summary>
        public string Plan { get; }

        /// <summary>
        /// The license price.
        /// </summary>
        public LicensePrice Price { get; }

        /// <summary>
        /// The license provider tenant id.
        /// </summary>
        public string ProviderId { get; }

        /// <summary>
        /// The license usage.
        /// </summary>
        public string Usage { get; }

        /// <summary>
        /// Disposes the license.
        /// </summary>
        public override void Dispose()
        {
        }

        private readonly string licenseKey;
    }
}