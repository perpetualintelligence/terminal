/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Abstractions;

namespace PerpetualIntelligence.Terminal.Licensing
{
    /// <summary>
    /// An abstraction to check the <see cref="License"/> object.
    /// </summary>
    public interface ILicenseChecker : IChecker<LicenseCheckerContext, LicenseCheckerResult>
    {
    }
}
