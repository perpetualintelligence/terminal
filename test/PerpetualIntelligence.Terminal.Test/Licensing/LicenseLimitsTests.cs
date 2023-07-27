/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Licensing;
using System;
using System.Collections.Generic;
using Xunit;

namespace PerpetualIntelligence.Terminal.Licensing
{
    public class LicenseLimitsTests
    {
        [Fact]
        public void CustomEdition_NoCustomClaimsShouldThrow()
        {
            Test.Services.TestHelper.AssertThrowsErrorException(() => LicenseLimits.Create(PiCliLicensePlans.Custom), "invalid_license", "The licensing for the custom SaaS plan requires a custom claims. saas_plan=urn:oneimlx:lic:plan:custom");
        }

        [Fact]
        public void CustomEdition_ShouldSetLimitsCorrectly()
        {
            Dictionary<string, object> claims = new();
            claims.Add("terminal_limit", 1);
            claims.Add("redistribution_limit", 2);
            claims.Add("root_command_limit", 3);
            claims.Add("grouped_command_limit", 4);
            claims.Add("sub_command_limit", 5);
            claims.Add("option_limit", 6);

            claims.Add("option_alias", true);
            claims.Add("default_option", false);
            claims.Add("default_option_value", true);
            claims.Add("strict_data_type", false);

            claims.Add("data_type_handlers", "");
            claims.Add("text_handlers", "t1");
            claims.Add("error_handlers", "e1 e2 e3");
            claims.Add("store_handlers", "st1 st2");
            claims.Add("service_handlers", "s1 s2 s3");
            claims.Add("license_handlers", "l1");

            LicenseLimits limits = LicenseLimits.Create(PiCliLicensePlans.Custom, claims);
            limits.Plan.Should().Be(PiCliLicensePlans.Custom);

            limits.TerminalLimit.Should().Be(1);
            limits.RedistributionLimit.Should().Be(2);
            limits.RootCommandLimit.Should().Be(3);
            limits.GroupedCommandLimit.Should().Be(4);
            limits.SubCommandLimit.Should().Be(5);
            limits.OptionLimit.Should().Be(6);

            limits.OptionAlias.Should().Be(true);
            limits.DefaultOption.Should().Be(false);
            limits.DefaultOptionValue.Should().Be(true);
            limits.StrictDataType.Should().Be(false);

            limits.DataTypeHandlers.Should().BeEquivalentTo(new string[] { "" });
            limits.TextHandlers.Should().BeEquivalentTo(new string[] { "t1" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "e1", "e2", "e3" });
            limits.StoreHandlers.Should().BeEquivalentTo(new string[] { "st1", "st2" });
            limits.ServiceHandlers.Should().BeEquivalentTo(new string[] { "s1", "s2", "s3" });
            limits.LicenseHandlers.Should().BeEquivalentTo(new string[] { "l1" });
        }

        [Fact]
        public void DemoClaims_ShouldSetClaimsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(PiCliLicensePlans.Demo);

            limits.Plan.Should().Be(PiCliLicensePlans.Demo);

            limits.TerminalLimit.Should().Be(1);
            limits.RedistributionLimit.Should().Be(0);
            limits.RootCommandLimit.Should().Be(1);
            limits.GroupedCommandLimit.Should().Be(5);
            limits.SubCommandLimit.Should().Be(25);
            limits.OptionLimit.Should().Be(500);

            limits.OptionAlias.Should().Be(true);
            limits.DefaultOption.Should().Be(true);
            limits.DefaultOptionValue.Should().Be(true);
            limits.StrictDataType.Should().Be(true);

            limits.DataTypeHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.TextHandlers.Should().BeEquivalentTo(new string[] { "unicode", "ascii" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.StoreHandlers.Should().BeEquivalentTo(new string[] { "in-memory", });
            limits.ServiceHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.LicenseHandlers.Should().BeEquivalentTo(new string[] { "online-license" });
        }

        [Fact]
        public void EnterpriseEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(PiCliLicensePlans.Enterprise);

            limits.Plan.Should().Be(PiCliLicensePlans.Enterprise);

            limits.TerminalLimit.Should().Be(5);
            limits.RedistributionLimit.Should().Be(5000);
            limits.RootCommandLimit.Should().Be(3);
            limits.GroupedCommandLimit.Should().Be(20);
            limits.SubCommandLimit.Should().Be(100);
            limits.OptionLimit.Should().Be(2000);

            limits.OptionAlias.Should().Be(true);
            limits.DefaultOption.Should().Be(true);
            limits.DefaultOptionValue.Should().Be(true);
            limits.StrictDataType.Should().Be(true);

            limits.DataTypeHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.TextHandlers.Should().BeEquivalentTo(new string[] { "unicode", "ascii" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.StoreHandlers.Should().BeEquivalentTo(new string[] { "in-memory", "json", "custom" });
            limits.ServiceHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.LicenseHandlers.Should().BeEquivalentTo(new string[] { "online-license", "offline-license" });
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
                ErrorException eex = (ErrorException)ex;
                eex.Error.ErrorCode.Should().Be(TerminalErrors.InvalidLicense);
                eex.Error.FormatDescription().Should().Be("The licensing for the SaaS plan is not supported. saas_plan=invalid_plan");
            }
        }

        [Fact]
        public void OnPremiseEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(PiCliLicensePlans.OnPremise);

            limits.Plan.Should().Be(PiCliLicensePlans.OnPremise);

            limits.TerminalLimit.Should().Be(25);
            limits.RedistributionLimit.Should().Be(10000);
            limits.RootCommandLimit.Should().Be(5);
            limits.GroupedCommandLimit.Should().Be(50);
            limits.SubCommandLimit.Should().Be(250);
            limits.OptionLimit.Should().Be(5000);

            limits.OptionAlias.Should().Be(true);
            limits.DefaultOption.Should().Be(true);
            limits.DefaultOptionValue.Should().Be(true);
            limits.StrictDataType.Should().Be(true);

            limits.DataTypeHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.TextHandlers.Should().BeEquivalentTo(new string[] { "unicode", "ascii" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.StoreHandlers.Should().BeEquivalentTo(new string[] { "in-memory", "json", "custom" });
            limits.ServiceHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.LicenseHandlers.Should().BeEquivalentTo(new string[] { "online-license", "offline-license", "onpremise-license" });
        }

        [Fact]
        public void UnlimitedEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(PiCliLicensePlans.Unlimited);

            limits.Plan.Should().Be(PiCliLicensePlans.Unlimited);

            limits.TerminalLimit.Should().Be(null);
            limits.RedistributionLimit.Should().Be(null);
            limits.RootCommandLimit.Should().Be(null);
            limits.GroupedCommandLimit.Should().Be(null);
            limits.SubCommandLimit.Should().Be(null);
            limits.OptionLimit.Should().Be(null);

            limits.OptionAlias.Should().Be(true);
            limits.DefaultOption.Should().Be(true);
            limits.DefaultOptionValue.Should().Be(true);
            limits.StrictDataType.Should().Be(true);

            limits.DataTypeHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.TextHandlers.Should().BeEquivalentTo(new string[] { "unicode", "ascii" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.StoreHandlers.Should().BeEquivalentTo(new string[] { "in-memory", "json", "custom" });
            limits.ServiceHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.LicenseHandlers.Should().BeEquivalentTo(new string[] { "online-license", "offline-license", "onpremise-license" });
        }

        [Fact]
        public void MicroEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(PiCliLicensePlans.Micro);

            limits.Plan.Should().Be(PiCliLicensePlans.Micro);

            limits.TerminalLimit.Should().Be(1);
            limits.RedistributionLimit.Should().Be(0);
            limits.RootCommandLimit.Should().Be(1);
            limits.GroupedCommandLimit.Should().Be(5);
            limits.SubCommandLimit.Should().Be(25);
            limits.OptionLimit.Should().Be(500);

            limits.OptionAlias.Should().Be(false);
            limits.DefaultOption.Should().Be(false);
            limits.DefaultOptionValue.Should().Be(false);
            limits.StrictDataType.Should().Be(false);

            limits.DataTypeHandlers.Should().BeNull();
            limits.TextHandlers.Should().BeEquivalentTo(new string[] { "unicode", "ascii" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.StoreHandlers.Should().BeEquivalentTo(new string[] { "in-memory" });
            limits.ServiceHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.LicenseHandlers.Should().BeEquivalentTo(new string[] { "online-license" });
        }

        [Fact]
        public void SMBEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(PiCliLicensePlans.SMB);

            limits.Plan.Should().Be(PiCliLicensePlans.SMB);

            limits.TerminalLimit.Should().Be(3);
            limits.RedistributionLimit.Should().Be(1000);
            limits.RootCommandLimit.Should().Be(1);
            limits.GroupedCommandLimit.Should().Be(10);
            limits.SubCommandLimit.Should().Be(50);
            limits.OptionLimit.Should().Be(1000);

            limits.OptionAlias.Should().Be(true);
            limits.DefaultOption.Should().Be(true);
            limits.DefaultOptionValue.Should().Be(true);
            limits.StrictDataType.Should().Be(true);

            limits.DataTypeHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.TextHandlers.Should().BeEquivalentTo(new string[] { "unicode", "ascii" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.StoreHandlers.Should().BeEquivalentTo(new string[] { "in-memory", "json" });
            limits.ServiceHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.LicenseHandlers.Should().BeEquivalentTo(new string[] { "online-license" });
        }

        [Fact]
        public void StandardEdition_ShouldIgnoreClaims()
        {
            Dictionary<string, object> expected = new()
            {
                { "terminal_limit", 25332343 },
                { "redistribution_limit", 36523211212212 },
                { "text_handlers", new[] { "new1", "new2" } }
            };

            LicenseLimits limits = LicenseLimits.Create(PiCliLicensePlans.Demo, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);
            limits.TextHandlers.Should().NotBeEquivalentTo(new[] { "new1", "new2" });

            limits = LicenseLimits.Create(PiCliLicensePlans.Micro, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);
            limits.TextHandlers.Should().NotBeEquivalentTo(new[] { "new1", "new2" });

            limits = LicenseLimits.Create(PiCliLicensePlans.SMB, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);
            limits.TextHandlers.Should().NotBeEquivalentTo(new[] { "new1", "new2" });

            limits = LicenseLimits.Create(PiCliLicensePlans.Enterprise, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);
            limits.TextHandlers.Should().NotBeEquivalentTo(new[] { "new1", "new2" });

            limits = LicenseLimits.Create(PiCliLicensePlans.OnPremise, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);
            limits.TextHandlers.Should().NotBeEquivalentTo(new[] { "new1", "new2" });

            limits = LicenseLimits.Create(PiCliLicensePlans.Unlimited, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);
            limits.TextHandlers.Should().NotBeEquivalentTo(new[] { "new1", "new2" });
        }
    }
}