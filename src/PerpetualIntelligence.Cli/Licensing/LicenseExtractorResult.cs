/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Collections.Generic;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// The default <see cref="ILicenseChecker"/> result.
    /// </summary>
    public class LicenseExtractorResult
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="licenses">The checked licensed.</param>
        public LicenseExtractorResult(IEnumerable<License> licenses)
        {
            Licenses = licenses ?? throw new System.ArgumentNullException(nameof(licenses));
        }

        /// <summary>
        /// The valid licenses.
        /// </summary>
        public IEnumerable<License> Licenses { get; }
    }
}
