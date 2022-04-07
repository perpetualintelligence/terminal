/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Licensing;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockLicenseExtractorInner : ILicenseExtractor
    {
        public License[] MultipleLicenses { get; set; } = new[]
        {
            new License("testKey1", MockLicenses.TestClaims, MockLicenses.TestLimits),
            new License("testKey2", MockLicenses.TestClaims, MockLicenses.TestLimits),
            new License("testKey3", MockLicenses.TestClaims, MockLicenses.TestLimits)
        };

        public bool NoLicense { get; set; }

        public License[] SingleLicense { get; set; } = new[] { new License("testKey", MockLicenses.TestClaims, MockLicenses.TestLimits) };

        public bool UseMultiple { get; set; }

        public Task<LicenseExtractorResult> ExtractAsync(LicenseExtractorContext context)
        {
            if (NoLicense)
            {
                return Task.FromResult(new LicenseExtractorResult(Array.Empty<License>()));
            }

            if (UseMultiple)
            {
                return Task.FromResult(new LicenseExtractorResult(MultipleLicenses));
            }
            else
            {
                return Task.FromResult(new LicenseExtractorResult(SingleLicense));
            }
        }
    }
}
