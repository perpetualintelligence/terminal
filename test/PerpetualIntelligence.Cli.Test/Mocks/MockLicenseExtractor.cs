/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Licensing;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockLicenseExtractor : ILicenseExtractor
    {
        public Task<LicenseExtractorResult> ExtractAsync(LicenseExtractorContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
