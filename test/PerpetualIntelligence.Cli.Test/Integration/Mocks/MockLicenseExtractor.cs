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
    public class MockLicenseExtractor : ILicenseExtractor
    {
        public ValueTuple<int, bool> ExtractLicenseCalled { get; set; }

        public Task<LicenseExtractorResult> ExtractAsync(LicenseExtractorContext context)
        {
            ExtractLicenseCalled = new(MockCliHostedServiceStaticCounter.Increment(), true);
            return Task.FromResult(new LicenseExtractorResult(MockLicenses.TestLicense));
        }

        public Task<License?> GetLicenseAsync()
        {
            return Task.FromResult<License?>(MockLicenses.TestLicense);
        }
    }
}
