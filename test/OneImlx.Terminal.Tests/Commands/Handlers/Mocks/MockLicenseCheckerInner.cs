/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Licensing;

namespace OneImlx.Terminal.Commands.Handlers.Mocks
{
    public class MockLicenseCheckerInner : ILicenseChecker
    {
        public bool Called { get; set; }

        public License? PassedLicense { get; set; }

        public Task<LicenseCheckerResult> CheckLicenseAsync(License license)
        {
            Called = true;
            PassedLicense = license;
            return Task.FromResult(new LicenseCheckerResult(license));
        }
    }
}
