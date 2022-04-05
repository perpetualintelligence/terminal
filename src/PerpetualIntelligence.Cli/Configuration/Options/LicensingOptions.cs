/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The licensing configuration options.
    /// </summary>
    public class LicensingOptions
    {
        /// <summary>
        /// The license keys.
        /// </summary>
        public string[]? LicenseKeys { get; set; }

        /// <summary>
        /// The license key source. Defaults to <see cref="LicenseKeySource.JwsFile"/>.
        /// </summary>
        public LicenseKeySource LicenseKeySource { get; set; } = LicenseKeySource.JwsFile;

        /// <summary>
        /// The signing public key used to validate the <see cref="LicenseKeys"/> if <see cref="LicenseKeySource"/> is
        /// set to <see cref="LicenseKeySource.JwsFile"/>.
        /// </summary>
        public string? SigningKey { get; set; }
    }
}
