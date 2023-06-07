/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Exceptions;

namespace PerpetualIntelligence.Terminal.Licensing
{
    /// <summary>
    /// The default <see cref="ILicenseChecker"/> result.
    /// </summary>
    public sealed class LicenseExtractorResult
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="license">The extracted license.</param>
        public LicenseExtractorResult(License license)
        {
            License = license ?? throw new ErrorException(Errors.InvalidLicense, "The extracted license cannot be null.");
        }

        /// <summary>
        /// The valid licenses.
        /// </summary>
        public License License { get; }
    }
}
