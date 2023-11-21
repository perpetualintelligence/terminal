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
        /// <param name="extractionHandler">The license handler used to extract the license. The value may be different from <see cref="License.Handler"/>. </param>
        public LicenseExtractorResult(License license, string extractionHandler)
        {
            License = license ?? throw new TerminalException(TerminalErrors.InvalidLicense, "The extracted license cannot be null.");
            ExtractionHandler = extractionHandler ?? throw new TerminalException(TerminalErrors.InvalidLicense, "The extraction license handler cannot be null.");
        }

        /// <summary>
        /// The valid licenses.
        /// </summary>
        public License License { get; }

        /// <summary>
        /// The license handler used to extract the license.
        /// </summary>
        /// <remarks>
        /// For <see cref="TerminalHandlers.OnPremiseLicenseHandler"/> license this value may be different from <see cref="License.Handler"/>.
        /// </remarks>
        public string ExtractionHandler { get; }
    }
}