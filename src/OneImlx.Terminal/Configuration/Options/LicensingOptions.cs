﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Shared;

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
    /// our test license for quick on-boarding and evaluation, please refer to <see href="https://docs.perpetualintelligence.com/articles/pi-demo/intro.html"/>.
    /// </para>
    /// </remarks>
    public sealed class LicensingOptions
    {
        /// <summary>
        /// The deployment environment. Defaults to <c>standard</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When set to <see cref="TerminalIdentifiers.AirGappedDeployment"/>, the <see cref="ILicenseExtractor"/> skips
        /// the license check. This setting is crucial for environments like secure data centers, hardware-centric
        /// locations, or factories, where Internet access is minimal or nonexistent. In such settings, the software and
        /// its configurations are locked to a device, making license file updates potentially restricted.
        /// </para>
        /// <para>
        /// <c>NOTE:</c> This option can be set if you have an active commercial plans
        /// <see cref="TerminalLicensePlans.Enterprise"/> or <see cref="TerminalLicensePlans.Corporate"/>. It is a
        /// violation of licensing terms to deploy or continue using the framework without an active commercial license plan.
        /// </para>
        /// </remarks>
        public string Deployment { get; set; } = TerminalIdentifiers.StandardDeployment;

        /// <summary>
        /// The license contents. Defaults to <c>null</c>.
        /// </summary>
        /// <remarks>
        /// For client side environments such as a Standalone WebAssembly you can set the license contents directly. If
        /// you set the license contents, the license file path will be ignored. You do however need to set the name of
        /// the license file in the <see cref="LicenseFile"/> property.
        /// </remarks>
        /// <see cref="TerminalServices.EncodeLicenseContents(string)"/>
        public string? LicenseContents { get; set; }

        /// <summary>
        /// The license file location.
        /// </summary>
        /// <remarks>
        /// NOTE: The online license mode is obsolete and will be removed in future release. Please use the offline
        /// license file.
        /// </remarks>
        public string LicenseFile { get; set; } = string.Empty;

        /// <summary>
        /// The license plan. Defaults to <see cref="TerminalLicensePlans.Demo"/>.
        /// </summary>
        public string LicensePlan { get; set; } = TerminalLicensePlans.Demo;
    }
}
