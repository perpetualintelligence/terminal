﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Shared;
using OneImlx.Test.FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace OneImlx.Terminal.Licensing
{
    public class LicenseQuotaTests
    {
        [Theory]
        [InlineData(ProductCatalog.TerminalPlanDemo)]
        [InlineData(ProductCatalog.TerminalPlanSolo)]
        [InlineData(ProductCatalog.TerminalPlanMicro)]
        [InlineData(ProductCatalog.TerminalPlanSmb)]
        [InlineData(ProductCatalog.TerminalPlanEnterprise)]
        [InlineData(ProductCatalog.TerminalPlanCorporate)]
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
            LicenseQuota quota = LicenseQuota.Create(ProductCatalog.TerminalPlanCorporate);
            quota.Plan.Should().Be(ProductCatalog.TerminalPlanCorporate);

            quota.Limits["terminals"].Should().Be(15);
            quota.Limits["commands"].Should().BeNull();
            quota.Limits["inputs"].Should().BeNull();
            quota.Limits["redistributions"].Should().BeNull();

            quota.Switches["datatype"].Should().Be(true);
            quota.Switches["driver"].Should().Be(true);
            quota.Switches["dynamics"].Should().Be(true);

            quota.Features["authentications"].Should().BeEquivalentTo(["msal", "oauth", "oidc", "none"]);
            quota.Features["encodings"].Should().BeEquivalentTo(["ascii", "utf8", "utf16", "utf32"]);
            quota.Features["stores"].Should().BeEquivalentTo(["memory", "custom"]);
            quota.Features["routers"].Should().BeEquivalentTo(["console", "tcp", "udp", "grpc", "http", "custom"]);
            quota.Features["deployments"].Should().BeEquivalentTo(["standard", "air_gapped"]);
        }

        [Fact]
        public void Custom_NoClaims_Throws()
        {
            Action act = () => LicenseQuota.Create(ProductCatalog.TerminalPlanCustom);
            act.Should().Throw<TerminalException>()
               .WithErrorCode(TerminalErrors.InvalidLicense)
               .WithErrorDescription("The licensing for the custom plan requires a custom claims. plan=urn:oneimlx:terminal:plan:custom");
        }

        [Fact]
        public void Custom_Sets_Quota_Correctly()
        {
            Dictionary<string, object> claims = new()
            {
                { "terminals", 1 },
                { "commands", 3 },
                { "inputs", 6 },
                { "redistributions", 2 },

                { "datatype", false },
                { "driver", true },
                { "dynamics", false },

                { "authentications", new[] { "abc", "xyz" } },
                { "encodings", new[] { "ascii", "utf8", "utf16", "utf32" } },
                { "stores", new[] { "memory", "custom" } },
                { "routers", new[] { "console", "tcp", "udp", "grpc", "http", "custom" } },
                { "deployments", new[] { "standard", "air_gapped" } }
            };

            LicenseQuota quota = LicenseQuota.Create(ProductCatalog.TerminalPlanCustom, claims);
            quota.Plan.Should().Be(ProductCatalog.TerminalPlanCustom);

            quota.Limits["terminals"].Should().Be(1);
            quota.Limits["commands"].Should().Be(3);
            quota.Limits["inputs"].Should().Be(6);
            quota.Limits["redistributions"].Should().Be(2);

            quota.Switches["datatype"].Should().Be(false);
            quota.Switches["driver"].Should().Be(true);
            quota.Switches["dynamics"].Should().Be(false);

            quota.Features["authentications"].Should().BeEquivalentTo(["abc", "xyz"]);
            quota.Features["encodings"].Should().BeEquivalentTo(["ascii", "utf8", "utf16", "utf32"]);
            quota.Features["stores"].Should().BeEquivalentTo(["memory", "custom"]);
            quota.Features["routers"].Should().BeEquivalentTo(["console", "tcp", "udp", "grpc", "http", "custom"]);
            quota.Features["deployments"].Should().BeEquivalentTo(["standard", "air_gapped"]);
        }

        [Fact]
        public void Demo_Sets_Quota_Correctly()
        {
            LicenseQuota quota = LicenseQuota.Create(ProductCatalog.TerminalPlanDemo);
            quota.Plan.Should().Be(ProductCatalog.TerminalPlanDemo);

            quota.Limits["terminals"].Should().Be(1);
            quota.Limits["commands"].Should().Be(25);
            quota.Limits["inputs"].Should().Be(250);
            quota.Limits["redistributions"].Should().Be(0);

            quota.Switches["datatype"].Should().Be(true);
            quota.Switches["driver"].Should().Be(true);
            quota.Switches["dynamics"].Should().Be(true);

            quota.Features["authentications"].Should().BeEquivalentTo(["msal", "oauth", "oidc", "none"]);
            quota.Features["encodings"].Should().BeEquivalentTo(["ascii", "utf8", "utf16", "utf32"]);
            quota.Features["stores"].Should().BeEquivalentTo(["memory"]);
            quota.Features["routers"].Should().BeEquivalentTo(["console", "tcp", "udp", "grpc", "http"]);
            quota.Features["deployments"].Should().BeEquivalentTo(["standard"]);
        }

        [Fact]
        public void Enterprise_Sets_Limits()
        {
            LicenseQuota quota = LicenseQuota.Create(ProductCatalog.TerminalPlanEnterprise);
            quota.Plan.Should().Be(ProductCatalog.TerminalPlanEnterprise);

            quota.Limits["terminals"].Should().Be(10);
            quota.Limits["commands"].Should().Be(300);
            quota.Limits["inputs"].Should().Be(6000);
            quota.Limits["redistributions"].Should().Be(15000);

            quota.Switches["datatype"].Should().Be(true);
            quota.Switches["driver"].Should().Be(true);
            quota.Switches["dynamics"].Should().Be(true);

            quota.Features["authentications"].Should().BeEquivalentTo(["msal", "oauth", "oidc", "none"]);
            quota.Features["encodings"].Should().BeEquivalentTo(["ascii", "utf8", "utf16", "utf32"]);
            quota.Features["stores"].Should().BeEquivalentTo(["memory", "custom"]);
            quota.Features["routers"].Should().BeEquivalentTo(["console", "tcp", "udp", "grpc", "http", "custom"]);
            quota.Features["deployments"].Should().BeEquivalentTo(["standard", "air_gapped"]);
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
            LicenseQuota quota = LicenseQuota.Create(ProductCatalog.TerminalPlanMicro);
            quota.Plan.Should().Be(ProductCatalog.TerminalPlanMicro);

            quota.Limits["terminals"].Should().Be(3);
            quota.Limits["commands"].Should().Be(50);
            quota.Limits["inputs"].Should().Be(500);
            quota.Limits["redistributions"].Should().Be(1000);

            quota.Switches["datatype"].Should().Be(true);
            quota.Switches["driver"].Should().Be(false);
            quota.Switches["dynamics"].Should().Be(false);

            quota.Features["authentications"].Should().BeEquivalentTo(["msal", "none"]);
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
                { "terminals", 1 },
                { "commands", 3 },
                { "inputs", 6 },
                { "redistributions", long.MaxValue },

                { "datatype", false },
                { "driver", true },
                { "dynamics", false },

                { "authentications", new[] { "msal", "oauth", "oidc" } },
                { "encodings", new[] { "ascii", "utf8", "utf16", "utf32" } },
                { "stores", new[] { "memory", "custom" } },
                { "routers", new[] { "console", "tcp", "udp", "grpc", "http", "custom" } },
                { "deployments", new[] { "standard", "air_gapped" } }
            };

            LicenseQuota quota = LicenseQuota.Create(ProductCatalog.TerminalPlanCustom, claims);
            quota.Plan.Should().Be(ProductCatalog.TerminalPlanCustom);

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
            quota.Deployments.Should().BeEquivalentTo(["standard", "air_gapped"]);
        }

        [Fact]
        public void Smb_Sets_Quota_Correctly()
        {
            LicenseQuota quota = LicenseQuota.Create(ProductCatalog.TerminalPlanSmb);
            quota.Plan.Should().Be(ProductCatalog.TerminalPlanSmb);

            quota.Limits["terminals"].Should().Be(5);
            quota.Limits["commands"].Should().Be(100);
            quota.Limits["inputs"].Should().Be(2000);
            quota.Limits["redistributions"].Should().Be(5000);

            quota.Switches["datatype"].Should().Be(true);
            quota.Switches["driver"].Should().Be(true);
            quota.Switches["dynamics"].Should().Be(false);

            quota.Features["authentications"].Should().BeEquivalentTo(["msal", "oauth", "oidc", "none"]);
            quota.Features["encodings"].Should().BeEquivalentTo(["ascii", "utf8", "utf16", "utf32"]);
            quota.Features["stores"].Should().BeEquivalentTo(["memory"]);
            quota.Features["routers"].Should().BeEquivalentTo(["console", "tcp", "udp"]);
            quota.Features["deployments"].Should().BeEquivalentTo(["standard"]);
        }

        [Fact]
        public void Solo_Sets_Quota_Correctly()
        {
            LicenseQuota quota = LicenseQuota.Create(ProductCatalog.TerminalPlanSolo);
            quota.Plan.Should().Be(ProductCatalog.TerminalPlanSolo);

            quota.Limits["terminals"].Should().Be(1);
            quota.Limits["commands"].Should().Be(25);
            quota.Limits["inputs"].Should().Be(250);
            quota.Limits["redistributions"].Should().Be(0);

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
