/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Exceptions;

namespace PerpetualIntelligence.Cli.Licensing
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
                throw new ErrorException(Errors.InvalidLicense, "The license cannot be null.");
            }

            License = license;
        }

        /// <summary>
        /// The terminal count.
        /// </summary>
        public long TerminalCount { get; set; }

        /// <summary>
        /// The option count.
        /// </summary>
        public long ArgumentCount { get; set; }

        /// <summary>
        /// The grouped command count.
        /// </summary>
        public long CommandGroupCount { get; set; }

        /// <summary>
        /// The valid license.
        /// </summary>
        public License License { get; }

        /// <summary>
        /// The root command count.
        /// </summary>
        public long RootCommandCount { get; set; }

        /// <summary>
        /// The sub command count.
        /// </summary>
        public long SubCommandCount { get; set; }
    }
}
