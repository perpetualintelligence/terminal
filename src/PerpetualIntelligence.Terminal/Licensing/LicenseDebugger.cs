/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Diagnostics;

namespace PerpetualIntelligence.Terminal.Licensing
{
    /// <summary>
    /// The default <see cref="ILicenseDebugger"/> that uses <see cref="Debugger"/>.
    /// </summary>
    public sealed class LicenseDebugger : ILicenseDebugger
    {
        /// <summary>
        /// Returns <see cref="Debugger.IsAttached"/> value.
        /// </summary>
        public bool IsDebuggerAttached()
        {
            return Debugger.IsAttached;
        }
    }
}