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
    public class LicenseLimitsTests
    {
        [Fact]
        public void CustomEdition_NoCustomClaimsShouldThrow()
        {
            Action act = () => LicenseLimits.Create(TerminalLicensePlans.Custom);
            act.Should().Throw<TerminalException>().WithMessage("The licensing for the custom plan requires a custom claims. plan=urn:oneimlx:terminal:plan:custom");
        }

        [Fact]
        public void CustomEdition_ShouldSetLimitsCorrectly()
        {
            Dictionary<string, object> claims = new()
            {
                { "terminal_limit", 1 },
                { "redistribution_limit", 2 },
                { "root_command_limit", 3 },
                { "grouped_command_limit", 4 },
                { "sub_command_limit", 5 },
                { "option_limit", 6 },

                { "strict_data_type", false },
                { "authentication", false },
            };

            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.Custom, claims);
            limits.Plan.Should().Be(TerminalLicensePlans.Custom);

            limits.TerminalLimit.Should().Be(1);
            limits.RedistributionLimit.Should().Be(2);
            limits.RootCommandLimit.Should().Be(3);
            limits.GroupedCommandLimit.Should().Be(4);
            limits.SubCommandLimit.Should().Be(5);
            limits.OptionLimit.Should().Be(6);

            limits.StrictDataType.Should().Be(false);
            limits.Authentication.Should().Be(false);
        }

        [Fact]
        public void DemoClaims_ShouldSetClaimsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.Demo);

            limits.Plan.Should().Be(TerminalLicensePlans.Demo);

            limits.TerminalLimit.Should().Be(1);
            limits.RedistributionLimit.Should().Be(0);
            limits.RootCommandLimit.Should().Be(1);
            limits.GroupedCommandLimit.Should().Be(5);
            limits.SubCommandLimit.Should().Be(25);
            limits.OptionLimit.Should().Be(500);

            limits.StrictDataType.Should().Be(true);
            limits.Authentication.Should().Be(true);
        }

        [Fact]
        public void EnterpriseEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.Enterprise);

            limits.Plan.Should().Be(TerminalLicensePlans.Enterprise);

            limits.TerminalLimit.Should().Be(5);
            limits.RedistributionLimit.Should().Be(5000);
            limits.RootCommandLimit.Should().Be(3);
            limits.GroupedCommandLimit.Should().Be(20);
            limits.SubCommandLimit.Should().Be(100);
            limits.OptionLimit.Should().Be(2000);

            limits.StrictDataType.Should().Be(true);
            limits.Authentication.Should().Be(true);
        }

        [Fact]
        public void InvalidEdition_ShouldError()
        {
            try
            {
                LicenseLimits limits = LicenseLimits.Create("invalid_plan");
            }
            catch (Exception ex)
            {
                TerminalException eex = (TerminalException)ex;
                eex.Error.ErrorCode.Should().Be(TerminalErrors.InvalidLicense);
                eex.Error.FormatDescription().Should().Be("The license for the plan is not supported. plan=invalid_plan");
            }
        }

        [Fact]
        public void OnPremiseEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.OnPremise);

            limits.Plan.Should().Be(TerminalLicensePlans.OnPremise);

            limits.TerminalLimit.Should().Be(25);
            limits.RedistributionLimit.Should().Be(10000);
            limits.RootCommandLimit.Should().Be(5);
            limits.GroupedCommandLimit.Should().Be(50);
            limits.SubCommandLimit.Should().Be(250);
            limits.OptionLimit.Should().Be(5000);

            limits.StrictDataType.Should().Be(true);
            limits.Authentication.Should().Be(true);
        }

        [Fact]
        public void UnlimitedEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.Unlimited);

            limits.Plan.Should().Be(TerminalLicensePlans.Unlimited);

            limits.TerminalLimit.Should().Be(null);
            limits.RedistributionLimit.Should().Be(null);
            limits.RootCommandLimit.Should().Be(null);
            limits.GroupedCommandLimit.Should().Be(null);
            limits.SubCommandLimit.Should().Be(null);
            limits.OptionLimit.Should().Be(null);

            limits.StrictDataType.Should().Be(true);
            limits.Authentication.Should().Be(true);
        }

        [Fact]
        public void MicroEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.Micro);

            limits.Plan.Should().Be(TerminalLicensePlans.Micro);

            limits.TerminalLimit.Should().Be(1);
            limits.RedistributionLimit.Should().Be(0);
            limits.RootCommandLimit.Should().Be(1);
            limits.GroupedCommandLimit.Should().Be(5);
            limits.SubCommandLimit.Should().Be(25);
            limits.OptionLimit.Should().Be(500);

            limits.StrictDataType.Should().Be(false);
            limits.Authentication.Should().Be(false);
        }

        [Fact]
        public void SMBEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.SMB);

            limits.Plan.Should().Be(TerminalLicensePlans.SMB);

            limits.TerminalLimit.Should().Be(3);
            limits.RedistributionLimit.Should().Be(1000);
            limits.RootCommandLimit.Should().Be(1);
            limits.GroupedCommandLimit.Should().Be(10);
            limits.SubCommandLimit.Should().Be(50);
            limits.OptionLimit.Should().Be(1000);

            limits.StrictDataType.Should().Be(true);
            limits.Authentication.Should().Be(false);
        }

        [Fact]
        public void StandardPlans_ShouldIgnoreClaims()
        {
            Dictionary<string, object> expected = new()
            {
                { "terminal_limit", 25332343 },
                { "redistribution_limit", 36523211212212 },
            };

            LicenseLimits limits = LicenseLimits.Create(TerminalLicensePlans.Demo, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);

            limits = LicenseLimits.Create(TerminalLicensePlans.Micro, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);

            limits = LicenseLimits.Create(TerminalLicensePlans.SMB, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);

            limits = LicenseLimits.Create(TerminalLicensePlans.Enterprise, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);

            limits = LicenseLimits.Create(TerminalLicensePlans.OnPremise, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);

            limits = LicenseLimits.Create(TerminalLicensePlans.Unlimited, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);
        }
    }
}