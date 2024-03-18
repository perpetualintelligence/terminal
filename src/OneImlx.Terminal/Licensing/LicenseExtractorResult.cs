/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Licensing
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
        /// <param name="extractionMode"></param>
        public LicenseExtractorResult(License license, string? extractionMode)
        {
            License = license ?? throw new TerminalException(TerminalErrors.InvalidLicense, "The extracted license cannot be null.");
            ExtractionMode = extractionMode;
        }

        /// <summary>
        /// The valid licenses.
        /// </summary>
        public License License { get; }

        /// <summary>
        /// The license mode used for extraction.
        /// </summary>
        public string? ExtractionMode { get; }
    }
}