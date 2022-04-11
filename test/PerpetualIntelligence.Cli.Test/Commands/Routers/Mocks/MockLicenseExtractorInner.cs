/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Protocols.Licensing;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockLicenseExtractorInner : ILicenseExtractor
    {
        public bool NoLicense { get; set; }

        public License TestLicense { get; set; } = new License("testProviderId", SaaSCheckModes.Online, SaaSPlans.Community, SaaSUsages.RnD, "testKey", MockLicenses.TestClaims, MockLicenses.TestLimits);

        public Task<LicenseExtractorResult> ExtractAsync(LicenseExtractorContext context)
        {
            if (NoLicense)
            {
                return Task.FromResult(new LicenseExtractorResult(null!));
            }

            return Task.FromResult(new LicenseExtractorResult(TestLicense));
        }
    }
}
