/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Licensing
{
    /// <summary>
    /// The default <see cref="ILicenseChecker"/> context.
    /// </summary>
    public sealed class LicenseCheckerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public LicenseCheckerContext(License license)
        {
            License = license ?? throw new System.ArgumentNullException(nameof(license));
        }

        /// <summary>
        /// The license to check.
        /// </summary>
        public License License { get; }
    }
}
