/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using FluentAssertions;
using OneImlx.Shared.Licensing;
using Xunit;

namespace OneImlx.Terminal.Licensing
{
    public class LicensePriceTests
    {
        [Fact]
        public void Corporate_Priced_Correctly()
        {
            LicensePrice price = LicensePrice.Create(TerminalLicensePlans.Corporate);
            price.Plan.Should().Be(TerminalLicensePlans.Corporate);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(4800);
            price.Yearly.Should().Be(52800);
        }

        [Fact]
        public void Custom_NoClaims_Throws()
        {
            Action act = static () => LicensePrice.Create(TerminalLicensePlans.Custom);
            act.Should().Throw<TerminalException>("The pricing for the custom SaaS plan requires a custom claims. saas_plan=urn:oneimlx:lic:plan:custom");
        }

        [Fact]
        public void Custom_Priced_Correctly()
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
        public void Demo_Priced_Correctly()
        {
            LicensePrice price = LicensePrice.Create(TerminalLicensePlans.Demo);
            price.Plan.Should().Be(TerminalLicensePlans.Demo);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(0);
            price.Yearly.Should().Be(0);
        }

        [Fact]
        public void Enterprise_Priced_Correctly()
        {
            LicensePrice price = LicensePrice.Create(TerminalLicensePlans.Enterprise);
            price.Plan.Should().Be(TerminalLicensePlans.Enterprise);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(2400);
            price.Yearly.Should().Be(26400);
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
        public void Micro_Priced_Correctly()
        {
            LicensePrice price = LicensePrice.Create(TerminalLicensePlans.Micro);
            price.Plan.Should().Be(TerminalLicensePlans.Micro);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(300);
            price.Yearly.Should().Be(3300);
        }

        [Fact]
        public void Smb_Priced_Correctly()
        {
            LicensePrice price = LicensePrice.Create(TerminalLicensePlans.Smb);
            price.Plan.Should().Be(TerminalLicensePlans.Smb);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(1200);
            price.Yearly.Should().Be(13200);
        }

        [Fact]
        public void Solo_Priced_Correctly()
        {
            LicensePrice price = LicensePrice.Create(TerminalLicensePlans.Solo);
            price.Plan.Should().Be(TerminalLicensePlans.Solo);
            price.Currency.Should().Be("USD");
            price.Monthly.Should().Be(50);
            price.Yearly.Should().Be(550);
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

            price = LicensePrice.Create(TerminalLicensePlans.Solo, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(TerminalLicensePlans.Micro, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(TerminalLicensePlans.Smb, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(TerminalLicensePlans.Enterprise, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);

            price = LicensePrice.Create(TerminalLicensePlans.Corporate, expected);
            price.Currency.Should().NotBe("INR");
            price.Monthly.Should().NotBe(36523.36);
            price.Yearly.Should().NotBe(251451544536523.36);
        }
    }
}
