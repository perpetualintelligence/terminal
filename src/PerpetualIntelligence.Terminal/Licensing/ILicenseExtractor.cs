/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Licensing
{
    /// <summary>
    /// An abstraction to extract the software <see cref="License"/>.
    /// </summary>
    public interface ILicenseExtractor
    {
        /// <summary>
        /// Extracts the <see cref="License"/> asynchronously.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<LicenseExtractorResult> ExtractAsync(LicenseExtractorContext context);

        /// <summary>
        /// Gets the extracted license asynchronously.
        /// </summary>
        public Task<License?> GetAsync();
    }
}