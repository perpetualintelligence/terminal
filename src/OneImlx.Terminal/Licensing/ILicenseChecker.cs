/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace OneImlx.Terminal.Licensing
{
    /// <summary>
    /// An abstraction to check the <see cref="License"/> object.
    /// </summary>
    public interface ILicenseChecker
    {
        /// <summary>
        /// Checks <see cref="License"/> asynchronously.
        /// </summary>
        /// <param name="license">The license to check.</param>
        /// <returns>The <see cref="LicenseCheckerResult"/> instance.</returns>
        public Task<LicenseCheckerResult> CheckLicenseAsync(License license);
    }
}
