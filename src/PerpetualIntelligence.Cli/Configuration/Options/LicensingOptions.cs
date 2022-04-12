/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Extensions;
using PerpetualIntelligence.Protocols.Licensing;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The licensing configuration options.
    /// </summary>
    public class LicensingOptions
    {
        /// <summary>
        /// The license check mode. Defaults to <see cref="SaaSCheckModes.Online"/>.
        /// </summary>
        public string CheckMode { get; set; } = SaaSCheckModes.Online;

        /// <summary>
        /// The license consumer tenant id.
        /// </summary>
        public string? ConsumerTenantId { get; set; }

        /// <summary>
        /// The HTTP client name for <see cref="SaaSCheckModes.Online"/> checks.
        /// </summary>
        /// <remarks>The name must match the <see cref="ICliBuilderExtensions.AddLicensingClient(Integration.ICliBuilder, string, System.TimeSpan)"/></remarks>
        public string? HttpClientName { get; set; }

        /// <summary>
        /// The license key source. Defaults to <see cref="SaaSKeySources.JsonFile"/>.
        /// </summary>
        public string KeySource { get; set; } = SaaSKeySources.JsonFile;

        /// <summary>
        /// The license key or the file containing license key.
        /// </summary>
        public string? LicenseKey { get; set; }

        /// <summary>
        /// The license SaaS provider id or the provider tenant id.
        /// </summary>
        public string? ProviderId { get; set; }

        /// <summary>
        /// The subject or a licensing context to check the license. Your subscription id or any other domain identifier
        /// usually establishes your licensing context.
        /// </summary>
        public string? Subject { get; set; }
    }
}
