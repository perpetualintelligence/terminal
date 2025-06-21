/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Shared;

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
        public LicenseExtractorResult(License license)
        {
            License = license ?? throw new TerminalException(TerminalErrors.InvalidLicense, "The extracted license cannot be null.");
        }

        /// <summary>
        /// The valid licenses.
        /// </summary>
        public License License { get; }
    }
}
