/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Licensing;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The licensing configuration options.
    /// </summary>
    /// <remarks>
    /// Please visit <see href="https://docs.perpetualintelligence.com/articles/pi-cli/licensing.html"/> to generate
    /// license keys and access your identifiers.
    /// <para>
    /// You will require a valid community or commercial license and identifiers to set the licensing options. To use
    /// our test license for quick onboarding and evaluation, please refer to <see href="https://docs.perpetualintelligence.com/articles/pi-demo/intro.html"/>.
    /// </para>
    /// </remarks>
    public class LicensingOptions
    {
        /// <summary>
        /// The authorized application id. This is also the <c>auth_apps</c> claim from your license key.
        /// </summary>
        public string? AuthorizedApplicationId { get; set; }

        /// <summary>
        /// The license consumer tenant id.
        /// </summary>
        public string? ConsumerTenantId { get; set; }

        /// <summary>
        /// The license key source. Defaults to <see cref="LicenseKeySources.JsonFile"/>.
        /// </summary>
        public string KeySource { get; set; } = LicenseKeySources.JsonFile;

        /// <summary>
        /// The license key or the file containing license key.
        /// </summary>
        /// <remarks>
        /// If <see cref="KeySource"/> is set to <see cref="LicenseKeySources.JsonFile"/>, then this option value must be a
        /// valid JSON file path containing license key.
        /// </remarks>
        public string? LicenseKey { get; set; }

        /// <summary>
        /// The license SaaS provider id or the provider tenant id. Defaults to <see cref="LicenseProviders.PerpetualIntelligence"/>
        /// </summary>
        public string? ProviderId { get; set; } = LicenseProviders.PerpetualIntelligence;

        /// <summary>
        /// The subject or a licensing context to check the license. Your subscription id or any other domain identifier
        /// usually establishes your licensing context.
        /// </summary>
        public string? Subject { get; set; }
    }
}
