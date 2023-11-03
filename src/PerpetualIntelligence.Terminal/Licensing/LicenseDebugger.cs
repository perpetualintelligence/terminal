/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace PerpetualIntelligence.Terminal.Licensing
{
    /// <summary>
    /// The default <see cref="ILicenseDebugger"/> that uses <see cref="Debugger"/>.
    /// </summary>
    public sealed class LicenseDebugger : ILicenseDebugger
    {
        private readonly ILogger<LicenseDebugger> logger;

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public LicenseDebugger(ILogger<LicenseDebugger> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Returns <see cref="Debugger.IsAttached"/> value.
        /// </summary>
        public bool IsDebuggerAttached()
        {
            bool attached = Debugger.IsAttached;
            logger.LogDebug("Check Debugger. attached={0}", attached);
            return attached;
        }
    }
}