/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using PerpetualIntelligence.Protocols.Licensing;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Collections.Generic;
using Xunit;

namespace PerpetualIntelligence.Cli.Licensing
{
    public class LicensePriceTests
    {
        [Fact]
        public void CommunityEdition_ShouldSetPriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(SaaSPlans.Community);
            price.Plan.Should().Be(SaaSPlans.Community);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(0);
            price.Yearly.Should().Be(0);
        }

        [Fact]
        public void CustomEdition_NoClaimsShouldError()
        {
            Test.Services.TestHelper.AssertThrowsErrorException(() => LicensePrice.Create(SaaSPlans.Custom), "invalid_license", "The pricing for the custom SaaS plan requires a custom claims. saas_plan=urn:oneimlx:lic:saasplan:custom");
        }

        [Fact]
        public void CustomEdition_PriceCorrectly()
        {
            Dictionary<string, object> expected = new Dictionary<string, object>()
            {
                {"currency", "INR" },
                {"monthly_price", 36523.36 },
                {"yearly_price", 251451544536523.36 },
            };

            LicensePrice price = LicensePrice.Create(SaaSPlans.Custom, expected);
            price.Currency.Should().Be("INR");
            price.Monthly.Should().Be(36523.36);
            price.Yearly.Should().Be(251451544536523.36);
        }

        [Fact]
        public void EnterpriseEdition_ShouldSetPriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(SaaSPlans.Enterprise);
            price.Plan.Should().Be(SaaSPlans.Enterprise);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(219);
            price.Yearly.Should().Be(2309);
        }

        [Fact]
        public void InvalidEdition_ShouldError()
        {
            try
            {
                LicensePrice price = LicensePrice.Create("invalid_plan");
            }
            catch (Exception ex)
            {
                ErrorException eex = (ErrorException)ex;
                eex.Error.ErrorCode.Should().Be(Errors.InvalidLicense);
                eex.Error.FormatDescription().Should().Be("The pricing for the SaaS plan is not supported. saas_plan=invalid_plan");
            }
        }

        [Fact]
        public void ISVEdition_PriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(SaaSPlans.ISV);
            price.Plan.Should().Be(SaaSPlans.ISV);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(619);
            price.Yearly.Should().Be(6609);
        }

        [Fact]
        public void ISVUEdition_PriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(SaaSPlans.ISVU);
            price.Plan.Should().Be(SaaSPlans.ISVU);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(1219);
            price.Yearly.Should().Be(13109);
        }

        [Fact]
        public void MicroEdition_PriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(SaaSPlans.Micro);
            price.Plan.Should().Be(SaaSPlans.Micro);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(19);
            price.Yearly.Should().Be(199);
        }

        [Fact]
        public void SMBEdition_PriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(SaaSPlans.SMB);
            price.Plan.Should().Be(SaaSPlans.SMB);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(119);
            price.Yearly.Should().Be(1209);
        }

        [Fact]
        public void StandardEdition_ShouldIgnoreClaims()
        {
            Dictionary<string, object> expected = new Dictionary<string, object>()
            {
                {"currency", "INR" },
                {"monthly_price", 36523.36 },
                {"yearly_price", 251451544536523.36 },
            };

            LicensePrice price = LicensePrice.Create(SaaSPlans.Community, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(SaaSPlans.Micro, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(SaaSPlans.SMB, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(SaaSPlans.Enterprise, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(SaaSPlans.ISV, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(SaaSPlans.ISVU, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);
        }
    }
}
