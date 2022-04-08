/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// The default <see cref="ILicenseChecker"/> context.
    /// </summary>
    public class LicenseCheckerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public LicenseCheckerContext(CommandDescriptor commandDescriptor, License license)
        {
            CommandDescriptor = commandDescriptor ?? throw new System.ArgumentNullException(nameof(commandDescriptor));
            License = license ?? throw new System.ArgumentNullException(nameof(license));
        }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public CommandDescriptor CommandDescriptor { get; }

        /// <summary>
        /// The license to check.
        /// </summary>
        public License License { get; }
    }
}
