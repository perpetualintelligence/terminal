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
    public class MockLicenseChecker : ILicenseChecker
    {
        public Task<LicenseCheckerResult> CheckAsync(LicenseCheckerContext context)
        {
            throw new NotImplementedException();
        }
    }
}
