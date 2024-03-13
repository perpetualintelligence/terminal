/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Licensing;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    public class MockLicenseExtractorInner : ILicenseExtractor
    {
        public bool NoLicense { get; set; }

        public License TestLicense { get; set; } = new License(TerminalHandlers.OnlineLicenseHandler, TerminalLicensePlans.Demo, LicenseUsage.RnD, "testKey", MockLicenses.TestClaims, MockLicenses.TestLimits, MockLicenses.TestPrice);

        public Task<LicenseExtractorResult> ExtractLicenseAsync(LicenseExtractorContext context)
        {
            if (NoLicense)
            {
                return Task.FromResult(new LicenseExtractorResult(null!, null!));
            }

            return Task.FromResult(new LicenseExtractorResult(TestLicense, TestLicense.Handler));
        }

        public Task<License?> GetLicenseAsync()
        {
            if (NoLicense)
            {
                return Task.FromResult<License?>(null);
            }
            else
            {
                return Task.FromResult<License?>(TestLicense);
            }
        }
    }
}