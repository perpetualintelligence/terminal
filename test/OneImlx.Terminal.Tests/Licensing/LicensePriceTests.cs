/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Shared.Licensing;
using System;
using System.Collections.Generic;
using Xunit;

namespace OneImlx.Terminal.Licensing
{
    public class LicensePriceTests
    {
        [Fact]
        public void CommunityEdition_ShouldSetPriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(TerminalLicensePlans.Demo);
            price.Plan.Should().Be(TerminalLicensePlans.Demo);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(0);
            price.Yearly.Should().Be(0);
        }

        [Fact]
        public void CustomEdition_NoClaimsShouldError()
        {
            Action act = static () => LicensePrice.Create(TerminalLicensePlans.Custom);
            act.Should().Throw<TerminalException>("The pricing for the custom SaaS plan requires a custom claims. saas_plan=urn:oneimlx:lic:plan:custom");
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

            LicensePrice price = LicensePrice.Create(TerminalLicensePlans.Custom, expected);
            price.Currency.Should().Be("INR");
            price.Monthly.Should().Be(36523.36);
            price.Yearly.Should().Be(251451544536523.36);
        }

        [Fact]
        public void EnterpriseEdition_PriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(TerminalLicensePlans.Enterprise);
            price.Plan.Should().Be(TerminalLicensePlans.Enterprise);
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
                TerminalException eex = (TerminalException)ex;
                eex.Error.ErrorCode.Should().Be(TerminalErrors.InvalidLicense);
                eex.Error.FormatDescription().Should().Be("The pricing for the plan is not supported. plan=invalid_plan");
            }
        }

        [Fact]
        public void OnPremiseEdition_PriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(TerminalLicensePlans.OnPremise);
            price.Plan.Should().Be(TerminalLicensePlans.OnPremise);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(1299);
            price.Yearly.Should().Be(14029);
        }

        [Fact]
        public void UnlimitedEdition_PriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(TerminalLicensePlans.Unlimited);
            price.Plan.Should().Be(TerminalLicensePlans.Unlimited);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(3299);
            price.Yearly.Should().Be(35629);
        }

        [Fact]
        public void MicroEdition_PriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(TerminalLicensePlans.Micro);
            price.Plan.Should().Be(TerminalLicensePlans.Micro);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(49);
            price.Yearly.Should().Be(529);
        }

        [Fact]
        public void SMBEdition_PriceCorrectly()
        {
            LicensePrice price = LicensePrice.Create(TerminalLicensePlans.SMB);
            price.Plan.Should().Be(TerminalLicensePlans.SMB);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(299);
            price.Yearly.Should().Be(3229);
        }

        [Fact]
        public void StandardEdition_ShouldIgnoreClaims()
        {
            Dictionary<string, object> expected = new()
            {
                {"currency", "INR" },
                {"monthly_price", 36523.36 },
                {"yearly_price", 251451544536523.36 },
            };

            LicensePrice price = LicensePrice.Create(TerminalLicensePlans.Demo, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(TerminalLicensePlans.Micro, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(TerminalLicensePlans.SMB, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(TerminalLicensePlans.Enterprise, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(TerminalLicensePlans.OnPremise, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(TerminalLicensePlans.Unlimited, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);
        }
    }
}