/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Shared.Licensing;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockLicenseExtractorInner : ILicenseExtractor
    {
        public bool NoLicense { get; set; }

        public License TestLicense { get; set; } = new License("testProviderId", Handlers.OnlineHandler, LicensePlans.Demo, LicenseUsages.RnD, LicenseSources.JsonFile, "testKey", MockLicenses.TestClaims, MockLicenses.TestLimits, MockLicenses.TestPrice);

        public Task<LicenseExtractorResult> ExtractAsync(LicenseExtractorContext context)
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
