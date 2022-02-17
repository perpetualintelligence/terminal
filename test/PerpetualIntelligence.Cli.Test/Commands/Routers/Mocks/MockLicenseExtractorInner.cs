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
        public bool NoLicense { get; set; }

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

        public License[] MultipleLicenses { get; set; } = new[]
        {
            new License("testKey1", MockClaimsPrincipal.NewEmpty()),
            new License("testKey2", MockClaimsPrincipal.NewEmpty()),
            new License("testKey3", MockClaimsPrincipal.NewEmpty())
        };

        public License[] SingleLicense { get; set; } = new[] { new License("testKey", MockClaimsPrincipal.NewEmpty()) };
    }
}
