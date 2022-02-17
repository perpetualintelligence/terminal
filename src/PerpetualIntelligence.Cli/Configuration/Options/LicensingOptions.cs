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
        /// The license key source. Defaults to <see cref="LicenseKeyType.PublicToken"/>.
        /// </summary>
        public LicenseKeyType LicenseKeyType { get; set; } = LicenseKeyType.PublicToken;

        /// <summary>
        /// The signing public key used to validate the <see cref="LicenseKeys"/> if <see cref="LicenseKeyType"/> is set
        /// to <see cref="LicenseKeyType.Jws"/>.
        /// </summary>
        public string? SigningKey { get; set; }
    }
}
