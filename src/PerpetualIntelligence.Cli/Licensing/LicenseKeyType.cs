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
    public enum LicenseKeyType
    {
        /// <summary>
        /// The license key is a signed JWT or JWS (JSON Web Signature).
        /// </summary>
        /// <remarks>
        /// The <see cref="Jws"/> is used for offline or online on-premise secured console apps. We do not recommend you
        /// use this option for your public console apps.The <see cref="Jws"/> key is bundled with your published
        /// console app. Your published or released public app will stop working if the JWS key is expired or invalidated.
        /// </remarks>
        Jws = 0,

        /// <summary>
        /// The license key is a public token without any secret.
        /// </summary>
        /// <remarks>
        /// The <see cref="PublicToken"/> is used to resolve the licensing info for public console apps. You can release
        /// your console apps to customers with the public token.
        /// </remarks>
        PublicToken = 0,
    }
}
