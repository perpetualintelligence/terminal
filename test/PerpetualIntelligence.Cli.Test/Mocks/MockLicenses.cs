/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Protocols.Licensing;
using System.Collections.Generic;

namespace PerpetualIntelligence.Cli.Mocks
{
    public static class MockLicenses
    {
        static MockLicenses()
        {
            TestClaims = LicenseClaims.Create(new Dictionary<string, object>()
            {
                {"name", "test_name" },
                {"country", "test_country" },
                {"aud", "test_audience" },
                {"iss", "test_issuer" },
                {"jti", "test_jti" },
                {"sub", "test_subject" },
                {"tid", "test_tenantid" },
                {"azp", "test_azp" },
                {"acr", "test_acr1 test_acr2" },
                {"exp", System.DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() },
                {"iat", System.DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds() },
                {"nbf",  System.DateTimeOffset.UtcNow.AddHours(-0.5).ToUnixTimeSeconds() },
            });

            TestLimits = LicenseLimits.Create(SaaSPlans.Community);

            MixValidInvalidLicense = new[]
            {
                new License("testLicKey1",  TestClaims, TestLimits),
                new License("invalidLicKey2", TestClaims, TestLimits),
                new License("testLicKey3", TestClaims, TestLimits),
                new License("invalidLicKey4", TestClaims, TestLimits)
            };

            SingleLicense = new[]
            {
                new License("testLicKey1", TestClaims, TestLimits)
            };
        }

        public static IEnumerable<License> MixValidInvalidLicense = null!;

        public static IEnumerable<License> SingleLicense = null!;

        public static LicenseClaims TestClaims = null!;

        public static LicenseLimits TestLimits = null!;
    }
}
