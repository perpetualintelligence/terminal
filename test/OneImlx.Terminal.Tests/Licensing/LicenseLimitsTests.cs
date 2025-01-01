/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Shared.Licensing;
using OneImlx.Test.FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace OneImlx.Terminal.Licensing
{
    public class LicenseLimitsTests
    {
        [Fact]
        public void Corporate_Sets_Limits_Correctly()
        {
            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.Corporate);

            limits.Plan.Should().Be(TerminalLicensePlans.Corporate);

            limits.TerminalLimit.Should().Be(15);
            limits.CommandLimit.Should().BeNull();
            limits.InputLimit.Should().BeNull();
            limits.RedistributionLimit.Should().BeNull();

            limits.StrictDataType.Should().Be(true);
            limits.Authentication.Should().Be(true);
        }

        [Fact]
        public void Custom_NoClaims_Throws()
        {
            Action act = static () => LicenseLimits.Create(TerminalLicensePlans.Custom);
            act.Should().Throw<TerminalException>()
               .WithErrorCode(TerminalErrors.InvalidLicense)
               .WithErrorDescription("The licensing for the custom plan requires a custom claims. plan=urn:oneimlx:terminal:plan:custom");
        }

        [Fact]
        public void Custom_Sets_Limits_Correctly()
        {
            Dictionary<string, object> claims = new()
            {
                { "terminal_limit", 1 },
                { "command_limit", 3 },
                { "input_limit", 6 },
                { "redistribution_limit", 2 },

                { "strict_data_type", false },
                { "authentication", true },
            };

            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.Custom, claims);
            limits.Plan.Should().Be(TerminalLicensePlans.Custom);

            limits.TerminalLimit.Should().Be(1);
            limits.CommandLimit.Should().Be(3);
            limits.InputLimit.Should().Be(6);
            limits.RedistributionLimit.Should().Be(2);

            limits.StrictDataType.Should().Be(false);
            limits.Authentication.Should().Be(true);
        }

        [Fact]
        public void Demo_Sets_Limits_Correctly()
        {
            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.Demo);

            limits.Plan.Should().Be(TerminalLicensePlans.Demo);

            limits.TerminalLimit.Should().Be(1);
            limits.CommandLimit.Should().Be(25);
            limits.InputLimit.Should().Be(250);
            limits.RedistributionLimit.Should().Be(0);

            limits.StrictDataType.Should().Be(true);
            limits.Authentication.Should().Be(true);
        }

        [Fact]
        public void Enterprise_Sets_Limits()
        {
            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.Enterprise);

            limits.Plan.Should().Be(TerminalLicensePlans.Enterprise);

            limits.TerminalLimit.Should().Be(10);
            limits.CommandLimit.Should().Be(300);
            limits.InputLimit.Should().Be(6000);
            limits.RedistributionLimit.Should().Be(15000);

            limits.StrictDataType.Should().Be(true);
            limits.Authentication.Should().Be(true);
        }

        [Fact]
        public void InvalidEdition_Throws()
        {
            Action func = () => LicenseLimits.Create("invalid_plan");
            func.Should().Throw<TerminalException>()
                .WithErrorCode(TerminalErrors.InvalidLicense)
                .WithErrorDescription("The license for the plan is not supported. plan=invalid_plan");
        }

        [Fact]
        public void Micro_Sets_Limits_Correctly()
        {
            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.Micro);

            limits.Plan.Should().Be(TerminalLicensePlans.Micro);

            limits.TerminalLimit.Should().Be(3);
            limits.CommandLimit.Should().Be(50);
            limits.InputLimit.Should().Be(500);
            limits.RedistributionLimit.Should().Be(1000);

            limits.StrictDataType.Should().Be(true);
            limits.Authentication.Should().Be(true);
        }

        [Fact]
        public void Smb_Sets_Limits_Correctly()
        {
            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.Smb);

            limits.Plan.Should().Be(TerminalLicensePlans.Smb);

            limits.TerminalLimit.Should().Be(5);
            limits.CommandLimit.Should().Be(100);
            limits.InputLimit.Should().Be(2000);
            limits.RedistributionLimit.Should().Be(5000);

            limits.StrictDataType.Should().Be(true);
            limits.Authentication.Should().Be(true);
        }

        [Fact]
        public void Solo_Sets_Limits_Correctly()
        {
            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.Solo);

            limits.Plan.Should().Be(TerminalLicensePlans.Solo);

            limits.TerminalLimit.Should().Be(1);
            limits.CommandLimit.Should().Be(25);
            limits.InputLimit.Should().Be(250);
            limits.RedistributionLimit.Should().Be(0);

            limits.StrictDataType.Should().Be(false);
            limits.Authentication.Should().Be(false);
        }

        [Fact]
        public void StandardPlans_Ignores_Custom_Claims()
        {
            Dictionary<string, object> expected = new()
            {
                { "terminal_limit", 25332343 },
                { "redistribution_limit", 36523211212212 },
            };

            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.Demo, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);

            limits = LicenseLimits.Create(TerminalLicensePlans.Solo, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);

            limits = LicenseLimits.Create(TerminalLicensePlans.Micro, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);

            limits = LicenseLimits.Create(TerminalLicensePlans.Smb, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);

            limits = LicenseLimits.Create(TerminalLicensePlans.Enterprise, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);

            limits = LicenseLimits.Create(TerminalLicensePlans.Corporate, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);
        }
    }
}
