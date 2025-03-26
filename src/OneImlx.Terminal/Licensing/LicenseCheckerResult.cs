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
    public sealed class LicenseCheckerResult
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="license">The checked license.</param>
        public LicenseCheckerResult(License license)
        {
            if (license == null)
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The license cannot be null.");
            }

            License = license;
        }

        /// <summary>
        /// The command count.
        /// </summary>
        public long CommandCount { get; set; }

        /// <summary>
        /// The input (argument and option) count.
        /// </summary>
        public long InputCount { get; set; }

        /// <summary>
        /// The valid license.
        /// </summary>
        public License License { get; }

        /// <summary>
        /// The terminal count.
        /// </summary>
        public long TerminalCount { get; set; }
    }
}
