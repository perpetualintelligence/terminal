/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Licensing
{
    /// <summary>
    /// An abstraction to check the <see cref="License"/> object.
    /// </summary>
    public interface ILicenseChecker
    {
        /// <summary>
        /// Checks <see cref="License"/> asynchronously.
        /// </summary>
        /// <param name="context">The license check context.</param>
        /// <returns>The <see cref="LicenseCheckerResult"/> instance.</returns>
        public Task<LicenseCheckerResult> CheckAsync(LicenseCheckerContext context);
    }
}