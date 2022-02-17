/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// The default <see cref="ILicenseChecker"/> result.
    /// </summary>
    public class LicenseCheckerResult
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="licenses">The checked licenses.</param>
        public LicenseCheckerResult(IEnumerable<License> licenses)
        {
            if (licenses == null || !licenses.Any())
            {
                throw new ErrorException(Errors.InvalidLicense, "The valid licenses cannot be null or empty.");
            }

            Licenses = licenses;
        }

        /// <summary>
        /// The valid licenses.
        /// </summary>
        public IEnumerable<License> Licenses { get; }
    }
}
