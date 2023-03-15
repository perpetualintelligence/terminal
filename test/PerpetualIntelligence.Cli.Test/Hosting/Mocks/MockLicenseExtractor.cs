/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Hosting.Mocks
{
    public class MockLicenseExtractor : ILicenseExtractor
    {
        public ValueTuple<int, bool> ExtractLicenseCalled { get; set; }

        public bool ThrowError { get; set; }

        public Task<LicenseExtractorResult> ExtractAsync(LicenseExtractorContext context)
        {
            ExtractLicenseCalled = new(MockCliHostedServiceStaticCounter.Increment(), true);

            if (ThrowError)
            {
                throw new ErrorException("test_error", "test description. arg1={0} arg2={1}", "val1", "val2");
            }

            return Task.FromResult(new LicenseExtractorResult(MockLicenses.TestLicense));
        }

        public Task<License?> GetLicenseAsync()
        {
            return Task.FromResult<License?>(MockLicenses.TestLicense);
        }
    }
}
