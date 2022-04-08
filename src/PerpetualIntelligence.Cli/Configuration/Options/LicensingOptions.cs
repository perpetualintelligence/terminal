/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Licensing;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The licensing configuration options.
    /// </summary>
    public class LicensingOptions
    {
        /// <summary>
        /// The license check mode. Defaults to <see cref="LicenseCheckMode.Online"/>.
        /// </summary>
        public LicenseCheckMode CheckMode { get; set; } = LicenseCheckMode.Online;

        /// <summary>
        /// The license consumer tenant id.
        /// </summary>
        public string? ConsumerTenantId { get; set; }

        /// <summary>
        /// The license key source. Defaults to <see cref="LicenseKeySource.JsonFile"/>.
        /// </summary>
        public LicenseKeySource KeySource { get; set; } = LicenseKeySource.JsonFile;

        /// <summary>
        /// The license key or the file containing license key.
        /// </summary>
        public string? LicenseKey { get; set; }

        /// <summary>
        /// The license provider tenant id.
        /// </summary>
        public string? ProviderTenantId { get; set; }

        /// <summary>
        /// The subject or a licensing context to check the license. Your subscription id or any other domain identifier
        /// usually establishes your licensing context.
        /// </summary>
        public string? Subject { get; set; }
    }
}
