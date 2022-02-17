/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands;
using System.Collections.Generic;

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
        public LicenseCheckerContext(LicenseCheckerFeature checkFeature, CommandDescriptor commandDescriptor, IEnumerable<License> licenses)
        {
            CheckFeature = checkFeature;
            CommandDescriptor = commandDescriptor ?? throw new System.ArgumentNullException(nameof(commandDescriptor));
            Licenses = licenses ?? throw new System.ArgumentNullException(nameof(licenses));
        }

        /// <summary>
        /// Determines which feature to check.
        /// </summary>
        public LicenseCheckerFeature CheckFeature { get; set; }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public CommandDescriptor CommandDescriptor { get; }

        /// <summary>
        /// The licenses to check.
        /// </summary>
        public IEnumerable<License> Licenses { get; }
    }
}
