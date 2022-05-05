/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Cli.Mocks;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Integration.Mocks
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
        public Task<LicenseCheckerResult> CheckAsync(LicenseCheckerContext context)
        {
            CheckLicenseCalled = new(MockCliHostedServiceStaticCounter.Increment(), true);
            return Task.FromResult(new LicenseCheckerResult(MockLicenses.TestLicense));
        }
    }
}
