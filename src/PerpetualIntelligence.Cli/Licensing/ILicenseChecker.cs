/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// An abstraction to check the <see cref="License"/> object.
    /// </summary>
    public interface ILicenseChecker : IChecker<LicenseCheckerContext, LicenseCheckerResult>
    {
    }
}
