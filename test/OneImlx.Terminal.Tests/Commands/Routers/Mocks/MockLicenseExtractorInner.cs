/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Mocks;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Routers.Mocks
{
    public class MockLicenseExtractorInner : ILicenseExtractor
    {
        public bool NoLicense { get; set; }

        public License TestLicense { get; set; } = new License(ProductCatalog.TerminalPlanDemo, LicenseUsage.RnD, "testKey", MockLicenses.TestClaims, MockLicenses.TestQuota);

        public Task<LicenseExtractorResult> ExtractLicenseAsync()
        {
            if (NoLicense)
            {
                return Task.FromResult(new LicenseExtractorResult(null!));
            }

            return Task.FromResult(new LicenseExtractorResult(TestLicense));
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
