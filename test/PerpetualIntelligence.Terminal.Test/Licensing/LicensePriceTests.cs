/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Shared.Licensing;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Collections.Generic;
using Xunit;

namespace PerpetualIntelligence.Terminal.Licensing
{
    public class LicensePriceTests
    {
        [Fact]
        public void CommunityEdition_ShouldSetPriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(LicensePlans.Demo);
            price.Plan.Should().Be(LicensePlans.Demo);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(0);
            price.Yearly.Should().Be(0);
        }

        [Fact]
        public void CustomEdition_NoClaimsShouldError()
        {
            Test.Services.TestHelper.AssertThrowsErrorException(() => LicensePrice.Create(LicensePlans.Custom), "invalid_license", "The pricing for the custom SaaS plan requires a custom claims. saas_plan=urn:oneimlx:lic:plan:custom");
        }

        [Fact]
        public void CustomEdition_PriceCorrectly()
        {
            Dictionary<string, object> expected = new()
            {
                {"currency", "INR" },
                {"monthly_price", 36523.36 },
                {"yearly_price", 251451544536523.36 },
            };

            LicensePrice price = LicensePrice.Create(LicensePlans.Custom, expected);
            price.Currency.Should().Be("INR");
            price.Monthly.Should().Be(36523.36);
            price.Yearly.Should().Be(251451544536523.36);
        }

        [Fact]
        public void EnterpriseEdition_PriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(LicensePlans.Enterprise);
            price.Plan.Should().Be(LicensePlans.Enterprise);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(699);
            price.Yearly.Should().Be(7529);
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
        public void OnPremiseEdition_PriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(LicensePlans.OnPremise);
            price.Plan.Should().Be(LicensePlans.OnPremise);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(1299);
            price.Yearly.Should().Be(14029);
        }

        [Fact]
        public void UnlimitedEdition_PriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(LicensePlans.Unlimited);
            price.Plan.Should().Be(LicensePlans.Unlimited);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(3299);
            price.Yearly.Should().Be(35629);
        }

        [Fact]
        public void MicroEdition_PriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(LicensePlans.Micro);
            price.Plan.Should().Be(LicensePlans.Micro);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(49);
            price.Yearly.Should().Be(529);
        }

        [Fact]
        public void SMBEdition_PriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(LicensePlans.SMB);
            price.Plan.Should().Be(LicensePlans.SMB);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(299);
            price.Yearly.Should().Be(3229);
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

            LicensePrice price = LicensePrice.Create(LicensePlans.Demo, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(LicensePlans.Micro, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(LicensePlans.SMB, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(LicensePlans.Enterprise, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(LicensePlans.OnPremise, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(LicensePlans.Unlimited, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);
        }
    }
}
