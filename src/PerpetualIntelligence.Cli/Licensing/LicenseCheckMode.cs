/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// The license checking mode.
    /// </summary>
    public enum LicenseCheckMode
    {
        /// <summary>
        /// The license check is done online via REST API.
        /// </summary>
        Online = 0,

        /// <summary>
        /// The license check is done offline via signed public keys. This is not yet supported.
        /// </summary>
        Offline = 1,

        /// <summary>
        /// First check <see cref="Online"/> then <see cref="Offline"/>. This is not yet supported.
        /// </summary>
        OnlineThenOffline = 2,

        /// <summary>
        /// First check <see cref="Offline"/> then <see cref="Online"/>. This is not yet supported.
        /// </summary>
        OfflineThenOnline = 2,
    }
}
