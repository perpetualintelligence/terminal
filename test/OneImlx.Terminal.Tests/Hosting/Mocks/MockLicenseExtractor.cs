/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Hosting.Mocks
{
    public class MockLicenseExtractor : ILicenseExtractor
    {
        public ValueTuple<int, bool> ExtractLicenseCalled { get; set; }

        public bool ThrowError { get; set; }

        public Task<LicenseExtractorResult> ExtractLicenseAsync()
        {
            ExtractLicenseCalled = new(MockTerminalHostedServiceStaticCounter.Increment(), true);

            if (ThrowError)
            {
                throw new TerminalException("test_error", "test description. opt1={0} opt2={1}", "val1", "val2");
            }

            return Task.FromResult(new LicenseExtractorResult(MockLicenses.TestLicense));
        }

        public Task<License?> GetLicenseAsync()
        {
            return Task.FromResult<License?>(MockLicenses.TestLicense);
        }
    }
}
