/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Licensing;
using System.Collections.Generic;

namespace PerpetualIntelligence.Cli.Mocks
{
    public static class MockLicenses
    {
        public static IEnumerable<License> MixValidInvalidLicense = new[]
       {
            new License("testLicKey1", MockClaimsPrincipal.NewEmpty()),
            new License("invalidLicKey2", MockClaimsPrincipal.NewEmpty()),
            new License("testLicKey3", MockClaimsPrincipal.NewEmpty()),
            new License("invalidLicKey4", MockClaimsPrincipal.NewEmpty())
        };
        public static IEnumerable<License> SingleLicense = new[]
                {
            new License("testLicKey1", MockClaimsPrincipal.NewEmpty())
        };
    }
}
