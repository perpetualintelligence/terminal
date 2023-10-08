/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Licensing;
using PerpetualIntelligence.Terminal.Mocks;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Hosting.Mocks
{
    /// <summary>
    /// </summary>
    public class MockLicenseChecker : ILicenseChecker
    {
        public ValueTuple<int, bool> CheckLicenseCalled { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<LicenseCheckerResult> CheckLicenseAsync(LicenseCheckerContext context)
        {
            CheckLicenseCalled = new(MockTerminalHostedServiceStaticCounter.Increment(), true);
            return Task.FromResult(new LicenseCheckerResult(MockLicenses.TestLicense));
        }
    }
}
