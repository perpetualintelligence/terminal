/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace OneImlx.Terminal.Licensing
{
    /// <summary>
    /// An abstraction to extract the software <see cref="License"/>.
    /// </summary>
    public interface ILicenseExtractor
    {
        /// <summary>
        /// Extracts the <see cref="License"/> asynchronously.
        /// </summary>
        /// <returns></returns>
        public Task<LicenseExtractorResult> ExtractLicenseAsync();

        /// <summary>
        /// Gets the extracted license asynchronously.
        /// </summary>
        public Task<License?> GetLicenseAsync();
    }
}
