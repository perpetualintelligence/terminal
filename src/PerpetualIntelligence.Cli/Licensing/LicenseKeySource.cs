/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Configuration.Options
{

    /// <summary>
    /// The license key source.
    /// </summary>
    public enum LicenseKeySource
    {
        /// <summary>
        /// A json file containing the signed JWT or JWS (JSON Web Signature) with claims identifying the features.
        /// </summary>
        JsonFile = 0,
    }
}
