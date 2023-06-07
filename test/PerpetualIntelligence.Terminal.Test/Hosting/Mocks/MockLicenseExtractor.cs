/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Licensing;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Hosting.Mocks
{
    public class MockLicenseExtractor : ILicenseExtractor
    {
        public ValueTuple<int, bool> ExtractLicenseCalled { get; set; }

        public bool ThrowError { get; set; }

        public Task<LicenseExtractorResult> ExtractAsync(LicenseExtractorContext context)
        {
            ExtractLicenseCalled = new(MockTerminalHostedServiceStaticCounter.Increment(), true);

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
