/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Licensing;
using OneImlx.Terminal.Licensing;
using System.Net.Http;

namespace OneImlx.Terminal.Configuration.Options
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
    public sealed class LicensingOptions
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
        /// The license key source. Defaults to <see cref="LicenseSources.JsonFile"/>.
        /// </summary>
        public string LicenseKeySource { get; set; } = LicenseSources.JsonFile;

        /// <summary>
        /// The license key or the file containing license key.
        /// </summary>
        /// <remarks>
        /// If <see cref="LicenseKeySource"/> is set to <see cref="LicenseSources.JsonFile"/>, then this option value must be a
        /// valid JSON file path containing license key.
        /// </remarks>
        public string? LicenseKey { get; set; }

        /// <summary>
        /// The subject or a licensing context to check the license. Your subscription id establishes your licensing context.
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// The license plan. Defaults to <see cref="TerminalLicensePlans.Demo"/>.
        /// </summary>
        public string LicensePlan { get; set; } = TerminalLicensePlans.Demo;

        /// <summary>
        /// The on-premise flag to deploy the terminal in a secured environment that is not connected to public internet.
        /// Defaults to <c>null</c>. If set to <c>true</c>, the <see cref="ILicenseExtractor"/> will not perform licensing check.
        /// This feature is critical in hardware centric or factory environments where the software stack and the configuration
        /// is locked down for a device.
        /// </summary>
        /// <remarks>
        /// This option can be set for <see cref="TerminalLicensePlans.OnPremise"/> or <see cref="TerminalLicensePlans.Unlimited"/>. It is a violation
        /// of licensing terms to deploy or continue using the framework without a valid active license plan.
        /// </remarks>
        public bool? OnPremiseDeployment { get; set; }

        /// <summary>
        /// The logical name to create and configure <see cref="HttpClient"/> instance for online licensing checks.
        /// </summary>
        /// <remarks>
        /// The framework uses <see cref="IHttpClientFactory.CreateClient(string)"/> and the configured name
        /// to create an instance of <see cref="HttpClient"/>.
        /// </remarks>
        public string? HttpClientName { get; set; }
    }
}