/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Licensing;

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
        /// The license file location.
        /// </summary>
        /// <remarks>
        /// NOTE: The online license mode is obsolete and will be removed in future release. Please use the offline license file.
        /// </remarks>
        public string? LicenseFile { get; set; }

        /// <summary>
        /// The license plan. Defaults to <see cref="TerminalLicensePlans.Demo"/>.
        /// </summary>
        public string LicensePlan { get; set; } = TerminalLicensePlans.Demo;

        /// <summary>
        /// The deployment environment. Defaults to <c>null</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When set to <see cref="TerminalIdentifiers.OnPremiseDeployment"/>, the <see cref="ILicenseExtractor"/> skips the license check.
        /// This setting is crucial for environments like secure data centers, hardware-centric locations, or factories, where internet access
        /// is minimal or nonexistent. In such settings, the software and its configurations are locked to a device, making license file updates
        /// potentially restricted.
        /// </para>
        /// <para>
        /// <c>NOTE:</c> This option can be set if you have commercial plans <see cref="TerminalLicensePlans.OnPremise"/> or <see cref="TerminalLicensePlans.Unlimited"/>.
        /// It is a violation of licensing terms to deploy or continue using the framework without an active commercial license plan.
        /// </para>
        /// </remarks>
        public string? Deployment { get; set; }
    }
}