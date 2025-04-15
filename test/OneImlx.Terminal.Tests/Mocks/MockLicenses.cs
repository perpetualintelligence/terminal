/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Licensing;
using System.Collections.Generic;

namespace OneImlx.Terminal.Mocks
{
    public static class MockLicenses
    {
        static MockLicenses()
        {
            TestClaims = LicenseClaims.Create(new Dictionary<string, object>()
            {
                {"name", "test_name" },
                {"oid", "test_id" },
                {"country", "test_country" },
                {"aud", "test_audience" },
                {"iss", "test_issuer" },
                {"jti", "test_jti" },
                {"sub", "test_subject" },
                {"tid", "test_tenantid" },
                {"azp", "test_azp" },
                {"acr", "test_acr1 test_acr2" },
                {"mode", "test_mode" },
                { "deployment", "test_deployment" },
                {"exp", System.DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() },
                {"iat", System.DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds() },
                {"nbf",  System.DateTimeOffset.UtcNow.AddHours(-0.5).ToUnixTimeSeconds() },
            });

            TestQuota = LicenseQuota.Create(ProductCatalog.TerminalPlanDemo);

            TestLicense = new License(ProductCatalog.TerminalPlanDemo, LicenseUsage.RnD, "testLicKey1", TestClaims, TestQuota);
        }

        public static LicenseClaims TestClaims = null!;
        public static License TestLicense = null!;
        public static LicenseQuota TestQuota = null!;
    }
}
