/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// The license features that are checked by <see cref="ILicenseChecker"/>.
    /// </summary>
    [Flags]
    public enum LicenseCheckerFeature
    {
        /// <summary>
        /// No feature to check.
        /// </summary>
        None = 0,

        /// <summary>
        /// Check license keys.
        /// </summary>
        LicenseKeys = 1,

        /// <summary>
        /// Check root command limit.
        /// </summary>
        RootCommandLimit = 2,

        /// <summary>
        /// Check command group limit.
        /// </summary>
        CommandGroupLimit = 4,

        /// <summary>
        /// Check command limit.
        /// </summary>
        CommandLimit = 8,
    }
}
