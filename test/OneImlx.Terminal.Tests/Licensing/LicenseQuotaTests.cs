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
        [Theory]
        [InlineData(TerminalLicensePlans.Demo)]
        [InlineData(TerminalLicensePlans.Solo)]
        [InlineData(TerminalLicensePlans.Micro)]
        [InlineData(TerminalLicensePlans.Smb)]
        [InlineData(TerminalLicensePlans.Enterprise)]
        [InlineData(TerminalLicensePlans.Corporate)]
        public void Claims_NoCustom_Throws(string plan)
        {
            Action act = () => LicenseQuota.Create(plan, new Dictionary<string, object>());
            act.Should().Throw<TerminalException>()
               .WithErrorCode(TerminalErrors.InvalidLicense)
               .WithErrorDescription($"The custom claims are valid only for custom plan. plan={plan}");
        }

        [Fact]
        public void Corporate_Sets_Quota_Correctly()
        {
            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Corporate);
            quota.Plan.Should().Be(TerminalLicensePlans.Corporate);

            quota.Limits["terminal"].Should().Be(15);
            quota.Limits["command"].Should().BeNull();
            quota.Limits["input"].Should().BeNull();
            quota.Limits["redistribution"].Should().BeNull();

            quota.Switches["datatype"].Should().Be(true);
            quota.Switches["driver"].Should().Be(true);
            quota.Switches["dynamics"].Should().Be(true);

            quota.Features["authentications"].Should().BeEquivalentTo(["msal", "oauth", "oidc"]);
            quota.Features["encodings"].Should().BeEquivalentTo(["ascii", "utf8", "utf16", "utf32"]);
            quota.Features["stores"].Should().BeEquivalentTo(["memory", "custom"]);
            quota.Features["routers"].Should().BeEquivalentTo(["console", "tcp", "udp", "grpc", "http", "custom"]);
            quota.Features["deployments"].Should().BeEquivalentTo(["standard", "isolated"]);
        }

        [Fact]
        public void Custom_NoClaims_Throws()
        {
            Action act = () => LicenseQuota.Create(TerminalLicensePlans.Custom);
            act.Should().Throw<TerminalException>()
               .WithErrorCode(TerminalErrors.InvalidLicense)
               .WithErrorDescription("The licensing for the custom plan requires a custom claims. plan=urn:oneimlx:terminal:plan:custom");
        }

        [Fact]
        public void Custom_Sets_Quota_Correctly()
        {
            Dictionary<string, object> claims = new()
            {
                { "terminal", 1 },
                { "command", 3 },
                { "input", 6 },
                { "redistribution", 2 },

                { "datatype", false },
                { "driver", true },
                { "dynamics", false },

                { "authentications", new[] { "abc", "xyz" } },
                { "encodings", new[] { "ascii", "utf8", "utf16", "utf32" } },
                { "stores", new[] { "memory", "custom" } },
                { "routers", new[] { "console", "tcp", "udp", "grpc", "http", "custom" } },
                { "deployments", new[] { "standard", "isolated" } }
            };

            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Custom, claims);
            quota.Plan.Should().Be(TerminalLicensePlans.Custom);

            quota.Limits["terminal"].Should().Be(1);
            quota.Limits["command"].Should().Be(3);
            quota.Limits["input"].Should().Be(6);
            quota.Limits["redistribution"].Should().Be(2);

            quota.Switches["datatype"].Should().Be(false);
            quota.Switches["driver"].Should().Be(true);
            quota.Switches["dynamics"].Should().Be(false);

            quota.Features["authentications"].Should().BeEquivalentTo(["abc", "xyz"]);
            quota.Features["encodings"].Should().BeEquivalentTo(["ascii", "utf8", "utf16", "utf32"]);
            quota.Features["stores"].Should().BeEquivalentTo(["memory", "custom"]);
            quota.Features["routers"].Should().BeEquivalentTo(["console", "tcp", "udp", "grpc", "http", "custom"]);
            quota.Features["deployments"].Should().BeEquivalentTo(["standard", "isolated"]);
        }

        [Fact]
        public void Demo_Sets_Quota_Correctly()
        {
            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Demo);
            quota.Plan.Should().Be(TerminalLicensePlans.Demo);

            quota.Limits["terminal"].Should().Be(1);
            quota.Limits["command"].Should().Be(25);
            quota.Limits["input"].Should().Be(250);
            quota.Limits["redistribution"].Should().Be(0);

            quota.Switches["datatype"].Should().Be(true);
            quota.Switches["driver"].Should().Be(true);
            quota.Switches["dynamics"].Should().Be(true);

            quota.Features["authentications"].Should().BeEquivalentTo(["msal", "oauth", "oidc"]);
            quota.Features["encodings"].Should().BeEquivalentTo(["ascii", "utf8", "utf16", "utf32"]);
            quota.Features["stores"].Should().BeEquivalentTo(["memory"]);
            quota.Features["routers"].Should().BeEquivalentTo(["console", "tcp", "udp", "grpc", "http"]);
            quota.Features["deployments"].Should().BeEquivalentTo(["standard"]);
        }

        [Fact]
        public void Enterprise_Sets_Limits()
        {
            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Enterprise);
            quota.Plan.Should().Be(TerminalLicensePlans.Enterprise);

            quota.Limits["terminal"].Should().Be(10);
            quota.Limits["command"].Should().Be(300);
            quota.Limits["input"].Should().Be(6000);
            quota.Limits["redistribution"].Should().Be(15000);

            quota.Switches["datatype"].Should().Be(true);
            quota.Switches["driver"].Should().Be(true);
            quota.Switches["dynamics"].Should().Be(true);

            quota.Features["authentications"].Should().BeEquivalentTo(["msal", "oauth", "oidc"]);
            quota.Features["encodings"].Should().BeEquivalentTo(["ascii", "utf8", "utf16", "utf32"]);
            quota.Features["stores"].Should().BeEquivalentTo(["memory", "custom"]);
            quota.Features["routers"].Should().BeEquivalentTo(["console", "tcp", "udp", "grpc", "http", "custom"]);
            quota.Features["deployments"].Should().BeEquivalentTo(["standard", "isolated"]);
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
        public void Micro_Sets_Quota_Correctly()
        {
            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Micro);
            quota.Plan.Should().Be(TerminalLicensePlans.Micro);

            quota.Limits["terminal"].Should().Be(3);
            quota.Limits["command"].Should().Be(50);
            quota.Limits["input"].Should().Be(500);
            quota.Limits["redistribution"].Should().Be(1000);

            quota.Switches["datatype"].Should().Be(true);
            quota.Switches["driver"].Should().Be(false);
            quota.Switches["dynamics"].Should().Be(false);

            quota.Features["authentications"].Should().BeEquivalentTo(["msal"]);
            quota.Features["encodings"].Should().BeEquivalentTo(["ascii", "utf8", "utf16", "utf32"]);
            quota.Features["stores"].Should().BeEquivalentTo(["memory"]);
            quota.Features["routers"].Should().BeEquivalentTo(["console"]);
            quota.Features["deployments"].Should().BeEquivalentTo(["standard"]);
        }

        [Fact]
        public void Properties_Returns_Correctly()
        {
            Dictionary<string, object> claims = new()
            {
                { "terminal", 1 },
                { "command", 3 },
                { "input", 6 },
                { "redistribution", long.MaxValue },

                { "datatype", false },
                { "driver", true },
                { "dynamics", false },

                { "authentications", new[] { "msal", "oauth", "oidc" } },
                { "encodings", new[] { "ascii", "utf8", "utf16", "utf32" } },
                { "stores", new[] { "memory", "custom" } },
                { "routers", new[] { "console", "tcp", "udp", "grpc", "http", "custom" } },
                { "deployments", new[] { "standard", "isolated" } }
            };

            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Custom, claims);
            quota.Plan.Should().Be(TerminalLicensePlans.Custom);

            quota.TerminalLimit.Should().Be(1);
            quota.CommandLimit.Should().Be(3);
            quota.InputLimit.Should().Be(6);
            quota.RedistributionLimit.Should().Be(long.MaxValue);

            quota.DataType.Should().BeFalse();
            quota.Driver.Should().BeTrue();
            quota.Dynamics.Should().BeFalse();

            quota.Authentications.Should().BeEquivalentTo(["msal", "oauth", "oidc"]);
            quota.Encodings.Should().BeEquivalentTo(["ascii", "utf8", "utf16", "utf32"]);
            quota.Stores.Should().BeEquivalentTo(["memory", "custom"]);
            quota.Routers.Should().BeEquivalentTo(["console", "tcp", "udp", "grpc", "http", "custom"]);
            quota.Deployments.Should().BeEquivalentTo(["standard", "isolated"]);
        }

        [Fact]
        public void Smb_Sets_Quota_Correctly()
        {
            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Smb);
            quota.Plan.Should().Be(TerminalLicensePlans.Smb);

            quota.Limits["terminal"].Should().Be(5);
            quota.Limits["command"].Should().Be(100);
            quota.Limits["input"].Should().Be(2000);
            quota.Limits["redistribution"].Should().Be(5000);

            quota.Switches["datatype"].Should().Be(true);
            quota.Switches["driver"].Should().Be(true);
            quota.Switches["dynamics"].Should().Be(false);

            quota.Features["authentications"].Should().BeEquivalentTo(["msal", "oauth", "oidc"]);
            quota.Features["encodings"].Should().BeEquivalentTo(["ascii", "utf8", "utf16", "utf32"]);
            quota.Features["stores"].Should().BeEquivalentTo(["memory"]);
            quota.Features["routers"].Should().BeEquivalentTo(["console", "tcp", "udp"]);
            quota.Features["deployments"].Should().BeEquivalentTo(["standard"]);
        }

        [Fact]
        public void Solo_Sets_Quota_Correctly()
        {
            LicenseQuota quota = LicenseQuota.Create(TerminalLicensePlans.Solo);
            quota.Plan.Should().Be(TerminalLicensePlans.Solo);

            quota.Limits["terminal"].Should().Be(1);
            quota.Limits["command"].Should().Be(25);
            quota.Limits["input"].Should().Be(250);
            quota.Limits["redistribution"].Should().Be(0);

            quota.Switches["datatype"].Should().Be(false);
            quota.Switches["driver"].Should().Be(false);
            quota.Switches["dynamics"].Should().Be(false);

            quota.Features["authentications"].Should().BeEquivalentTo(["none"]);
            quota.Features["encodings"].Should().BeEquivalentTo(["ascii", "utf8", "utf16", "utf32"]);
            quota.Features["stores"].Should().BeEquivalentTo(["memory"]);
            quota.Features["routers"].Should().BeEquivalentTo(["console"]);
            quota.Features["deployments"].Should().BeEquivalentTo(["standard"]);
        }
    }
}
