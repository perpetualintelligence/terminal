/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Licensing
{
    /// <summary>
    /// An abstraction of a license debugger.
    /// </summary>
    public interface ILicenseDebugger
    {
        /// <summary>
        /// Determines if the debugger is attached.
        /// </summary>
        public bool IsDebuggerAttached();
    }
}