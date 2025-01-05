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
    public class LicenseQuotaTests
    {
        [Fact]
        public void Corporate_Sets_Limits_Correctly()
        {
            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Corporate);

            quota.Plan.Should().Be(TerminalLicensePlans.Corporate);

            quota.Limits["terminal"].Should().Be(15);
            quota.Limits["command"].Should().BeNull();
            quota.Limits["input"].Should().BeNull();
            quota.Limits["redistribution"].Should().BeNull();

            quota.Switches["datatype"].Should().Be(true);
            quota.Switches["driver"].Should().Be(true);
            quota.Switches["integration"].Should().Be(true);

            quota.Features["authentication"].Should().BeEquivalentTo(["msal", "oauth", "oidc"]);
            quota.Features["encoding"].Should().BeEquivalentTo(["ascii", "utf8", "utf16-le", "utf16-be", "utf32"]);
            quota.Features["store"].Should().BeEquivalentTo(["memory", "custom"]);
            quota.Features["router"].Should().BeEquivalentTo(["console", "tcp", "udp", "grpc", "http", "custom"]);
            quota.Features["deployment"].Should().BeEquivalentTo(["standard", "isolated"]);
        }

        [Fact]
        public void Custom_NoClaims_Throws()
        {
            Action act = static () => LicenseQuota.Create(TerminalLicensePlans.Custom);
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
                { "driver", true },
                { "integration", false },
            };

            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Custom, claims);
            quota.Plan.Should().Be(TerminalLicensePlans.Custom);

            quota.TerminalLimit.Should().Be(1);
            quota.CommandLimit.Should().Be(3);
            quota.InputLimit.Should().Be(6);
            quota.RedistributionLimit.Should().Be(2);

            quota.StrictDataType.Should().Be(false);
            quota.Driver.Should().Be(true);
            quota.Integration.Should().Be(false);
        }

        [Fact]
        public void Demo_Sets_Limits_Correctly()
        {
            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Demo);

            quota.Plan.Should().Be(TerminalLicensePlans.Demo);

            quota.TerminalLimit.Should().Be(1);
            quota.CommandLimit.Should().Be(25);
            quota.InputLimit.Should().Be(250);
            quota.RedistributionLimit.Should().Be(0);

            quota.StrictDataType.Should().Be(true);
            quota.Driver.Should().Be(true);
            quota.Integration.Should().Be(true);
        }

        [Fact]
        public void Enterprise_Sets_Limits()
        {
            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Enterprise);

            quota.Plan.Should().Be(TerminalLicensePlans.Enterprise);

            quota.TerminalLimit.Should().Be(10);
            quota.CommandLimit.Should().Be(300);
            quota.InputLimit.Should().Be(6000);
            quota.RedistributionLimit.Should().Be(15000);

            quota.StrictDataType.Should().Be(true);
            quota.Driver.Should().Be(true);
            quota.Integration.Should().Be(true);
        }

        [Fact]
        public void InvalidEdition_Throws()
        {
            Action func = () => LicenseQuota.Create("invalid_plan");
            func.Should().Throw<TerminalException>()
                .WithErrorCode(TerminalErrors.InvalidLicense)
                .WithErrorDescription("The license for the plan is not supported. plan=invalid_plan");
        }

        [Fact]
        public void Micro_Sets_Limits_Correctly()
        {
            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Micro);

            quota.Plan.Should().Be(TerminalLicensePlans.Micro);

            quota.TerminalLimit.Should().Be(3);
            quota.CommandLimit.Should().Be(50);
            quota.InputLimit.Should().Be(500);
            quota.RedistributionLimit.Should().Be(1000);

            quota.StrictDataType.Should().Be(true);
            quota.Driver.Should().Be(false);
            quota.Integration.Should().Be(false);
        }

        [Fact]
        public void Smb_Sets_Limits_Correctly()
        {
            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Smb);

            quota.Plan.Should().Be(TerminalLicensePlans.Smb);

            quota.TerminalLimit.Should().Be(5);
            quota.CommandLimit.Should().Be(100);
            quota.InputLimit.Should().Be(2000);
            quota.RedistributionLimit.Should().Be(5000);

            quota.StrictDataType.Should().Be(true);
            quota.Driver.Should().Be(true);
            quota.Integration.Should().Be(false);
        }

        [Fact]
        public void Solo_Sets_Limits_Correctly()
        {
            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Solo);

            quota.Plan.Should().Be(TerminalLicensePlans.Solo);

            quota.TerminalLimit.Should().Be(1);
            quota.CommandLimit.Should().Be(25);
            quota.InputLimit.Should().Be(250);
            quota.RedistributionLimit.Should().Be(0);

            quota.StrictDataType.Should().Be(false);
            quota.Driver.Should().Be(false);
            quota.Integration.Should().Be(false);
        }

        [Fact]
        public void StandardPlans_Ignores_Custom_Claims()
        {
            Dictionary<string, object> expected = new()
            {
                { "terminal_limit", 25332343 },
                { "redistribution_limit", 36523211212212 },
            };

            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Demo, expected);
            quota.TerminalLimit.Should().NotBe(25332343);
            quota.RedistributionLimit.Should().NotBe(36523211212212);

            quota = LicenseQuota.Create(TerminalLicensePlans.Solo, expected);
            quota.TerminalLimit.Should().NotBe(25332343);
            quota.RedistributionLimit.Should().NotBe(36523211212212);

            quota = LicenseQuota.Create(TerminalLicensePlans.Micro, expected);
            quota.TerminalLimit.Should().NotBe(25332343);
            quota.RedistributionLimit.Should().NotBe(36523211212212);

            quota = LicenseQuota.Create(TerminalLicensePlans.Smb, expected);
            quota.TerminalLimit.Should().NotBe(25332343);
            quota.RedistributionLimit.Should().NotBe(36523211212212);

            quota = LicenseQuota.Create(TerminalLicensePlans.Enterprise, expected);
            quota.TerminalLimit.Should().NotBe(25332343);
            quota.RedistributionLimit.Should().NotBe(36523211212212);

            quota = LicenseQuota.Create(TerminalLicensePlans.Corporate, expected);
            quota.TerminalLimit.Should().NotBe(25332343);
            quota.RedistributionLimit.Should().NotBe(36523211212212);
        }
    }
}
