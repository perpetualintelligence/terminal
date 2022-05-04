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
    public class LicenseLimitsTests
    {
        [Fact]
        public void CommunityEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.Community);

            limits.Plan.Should().Be(SaaSPlans.Community);

            limits.TerminalLimit.Should().Be(1);
            limits.RedistributionLimit.Should().Be(0);
            limits.RootCommandLimit.Should().Be(1);
            limits.GroupedCommandLimit.Should().Be(5);
            limits.SubCommandLimit.Should().Be(25);
            limits.ArgumentLimit.Should().Be(500);

            limits.ArgumentAlias.Should().Be(false);
            limits.DefaultArgument.Should().Be(false);
            limits.DefaultArgumentValue.Should().Be(false);
            limits.StrictDataType.Should().Be(false);

            limits.DataTypeHandlers.Should().BeNull();
            limits.TextHandlers.Should().BeEquivalentTo(new string[] { "unicode", "ascii" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.StoreHandlers.Should().BeEquivalentTo(new string[] { "in-memory" });
            limits.ServiceHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.LicenseHandlers.Should().BeEquivalentTo(new string[] { "online" });
        }

        [Fact]
        public void CustomEdition_NoCustomClaimsShouldThrow()
        {
            Test.Services.TestHelper.AssertThrowsErrorException(() => LicenseLimits.Create(SaaSPlans.Custom), "invalid_license", "The licensing for the custom SaaS plan requires a custom claims. saas_plan=urn:oneimlx:lic:saasplan:custom");
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
            claims.Add("argument_limit", 6);

            claims.Add("argument_alias", true);
            claims.Add("default_argument", false);
            claims.Add("default_argument_value", true);
            claims.Add("strict_data_type", false);

            claims.Add("data_type_handlers", "");
            claims.Add("text_handlers", "t1");
            claims.Add("error_handlers", "e1 e2 e3");
            claims.Add("store_handlers", "st1 st2");
            claims.Add("service_handlers", "s1 s2 s3");
            claims.Add("license_handlers", "l1");

            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.Custom, claims);
            limits.Plan.Should().Be(SaaSPlans.Custom);

            limits.TerminalLimit.Should().Be(1);
            limits.RedistributionLimit.Should().Be(2);
            limits.RootCommandLimit.Should().Be(3);
            limits.GroupedCommandLimit.Should().Be(4);
            limits.SubCommandLimit.Should().Be(5);
            limits.ArgumentLimit.Should().Be(6);

            limits.ArgumentAlias.Should().Be(true);
            limits.DefaultArgument.Should().Be(false);
            limits.DefaultArgumentValue.Should().Be(true);
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
            var demoClaims = LicenseLimits.DemoClaims();

            demoClaims.Should().HaveCount(19);

            demoClaims.Should().Contain(new KeyValuePair<string, object>("terminal_limit", 1));
            demoClaims.Should().Contain(new KeyValuePair<string, object>("redistribution_limit", 0));
            demoClaims.Should().Contain(new KeyValuePair<string, object>("root_command_limit", 1));
            demoClaims.Should().Contain(new KeyValuePair<string, object>("grouped_command_limit", 2));
            demoClaims.Should().Contain(new KeyValuePair<string, object>("sub_command_limit", 15));
            demoClaims.Should().Contain(new KeyValuePair<string, object>("argument_limit", 100));

            demoClaims.Should().Contain(new KeyValuePair<string, object>("argument_alias", true));
            demoClaims.Should().Contain(new KeyValuePair<string, object>("default_argument", true));
            demoClaims.Should().Contain(new KeyValuePair<string, object>("default_argument_value", true));
            demoClaims.Should().Contain(new KeyValuePair<string, object>("strict_data_type", true));

            demoClaims.Should().Contain(new KeyValuePair<string, object>("data_type_handlers", "default"));
            demoClaims.Should().Contain(new KeyValuePair<string, object>("text_handlers", "unicode"));
            demoClaims.Should().Contain(new KeyValuePair<string, object>("error_handlers", "default"));
            demoClaims.Should().Contain(new KeyValuePair<string, object>("store_handlers", "in-memory"));
            demoClaims.Should().Contain(new KeyValuePair<string, object>("service_handlers", "default"));
            demoClaims.Should().Contain(new KeyValuePair<string, object>("license_handlers", "online"));

            demoClaims.Should().Contain(new KeyValuePair<string, object>("currency", "USD"));
            demoClaims.Should().Contain(new KeyValuePair<string, object>("monthly_price", 0.0));
            demoClaims.Should().Contain(new KeyValuePair<string, object>("yearly_price", 0.0));
        }

        [Fact]
        public void EnterpriseEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.Enterprise);

            limits.Plan.Should().Be(SaaSPlans.Enterprise);

            limits.TerminalLimit.Should().Be(5);
            limits.RedistributionLimit.Should().Be(5000);
            limits.RootCommandLimit.Should().Be(3);
            limits.GroupedCommandLimit.Should().Be(20);
            limits.SubCommandLimit.Should().Be(100);
            limits.ArgumentLimit.Should().Be(2000);

            limits.ArgumentAlias.Should().Be(true);
            limits.DefaultArgument.Should().Be(true);
            limits.DefaultArgumentValue.Should().Be(true);
            limits.StrictDataType.Should().Be(true);

            limits.DataTypeHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.TextHandlers.Should().BeEquivalentTo(new string[] { "unicode", "ascii" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.StoreHandlers.Should().BeEquivalentTo(new string[] { "in-memory", "json", "custom" });
            limits.ServiceHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.LicenseHandlers.Should().BeEquivalentTo(new string[] { "online", "offline" });
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
                eex.Error.ErrorCode.Should().Be(Errors.InvalidLicense);
                eex.Error.FormatDescription().Should().Be("The licensing for the SaaS plan is not supported. saas_plan=invalid_plan");
            }
        }

        [Fact]
        public void ISVEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.ISV);

            limits.Plan.Should().Be(SaaSPlans.ISV);

            limits.TerminalLimit.Should().Be(25);
            limits.RedistributionLimit.Should().Be(10000);
            limits.RootCommandLimit.Should().Be(5);
            limits.GroupedCommandLimit.Should().Be(50);
            limits.SubCommandLimit.Should().Be(250);
            limits.ArgumentLimit.Should().Be(5000);

            limits.ArgumentAlias.Should().Be(true);
            limits.DefaultArgument.Should().Be(true);
            limits.DefaultArgumentValue.Should().Be(true);
            limits.StrictDataType.Should().Be(true);

            limits.DataTypeHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.TextHandlers.Should().BeEquivalentTo(new string[] { "unicode", "ascii" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.StoreHandlers.Should().BeEquivalentTo(new string[] { "in-memory", "json", "custom" });
            limits.ServiceHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.LicenseHandlers.Should().BeEquivalentTo(new string[] { "online", "offline", "byol" });
        }

        [Fact]
        public void ISVUEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.ISVU);

            limits.Plan.Should().Be(SaaSPlans.ISVU);

            limits.TerminalLimit.Should().Be(null);
            limits.RedistributionLimit.Should().Be(null);
            limits.RootCommandLimit.Should().Be(null);
            limits.GroupedCommandLimit.Should().Be(null);
            limits.SubCommandLimit.Should().Be(null);
            limits.ArgumentLimit.Should().Be(null);

            limits.ArgumentAlias.Should().Be(true);
            limits.DefaultArgument.Should().Be(true);
            limits.DefaultArgumentValue.Should().Be(true);
            limits.StrictDataType.Should().Be(true);

            limits.DataTypeHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.TextHandlers.Should().BeEquivalentTo(new string[] { "unicode", "ascii" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.StoreHandlers.Should().BeEquivalentTo(new string[] { "in-memory", "json", "custom" });
            limits.ServiceHandlers.Should().BeEquivalentTo(new string[] { "default", "custom" });
            limits.LicenseHandlers.Should().BeEquivalentTo(new string[] { "online", "offline", "byol" });
        }

        [Fact]
        public void MicroEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.Micro);

            limits.Plan.Should().Be(SaaSPlans.Micro);

            limits.TerminalLimit.Should().Be(1);
            limits.RedistributionLimit.Should().Be(0);
            limits.RootCommandLimit.Should().Be(1);
            limits.GroupedCommandLimit.Should().Be(5);
            limits.SubCommandLimit.Should().Be(25);
            limits.ArgumentLimit.Should().Be(500);

            limits.ArgumentAlias.Should().Be(false);
            limits.DefaultArgument.Should().Be(false);
            limits.DefaultArgumentValue.Should().Be(false);
            limits.StrictDataType.Should().Be(false);

            limits.DataTypeHandlers.Should().BeNull();
            limits.TextHandlers.Should().BeEquivalentTo(new string[] { "unicode", "ascii" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.StoreHandlers.Should().BeEquivalentTo(new string[] { "in-memory" });
            limits.ServiceHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.LicenseHandlers.Should().BeEquivalentTo(new string[] { "online" });
        }

        [Fact]
        public void SMBEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.SMB);

            limits.Plan.Should().Be(SaaSPlans.SMB);

            limits.TerminalLimit.Should().Be(3);
            limits.RedistributionLimit.Should().Be(1000);
            limits.RootCommandLimit.Should().Be(1);
            limits.GroupedCommandLimit.Should().Be(10);
            limits.SubCommandLimit.Should().Be(50);
            limits.ArgumentLimit.Should().Be(1000);

            limits.ArgumentAlias.Should().Be(true);
            limits.DefaultArgument.Should().Be(true);
            limits.DefaultArgumentValue.Should().Be(true);
            limits.StrictDataType.Should().Be(true);

            limits.DataTypeHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.TextHandlers.Should().BeEquivalentTo(new string[] { "unicode", "ascii" });
            limits.ErrorHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.StoreHandlers.Should().BeEquivalentTo(new string[] { "in-memory", "json" });
            limits.ServiceHandlers.Should().BeEquivalentTo(new string[] { "default" });
            limits.LicenseHandlers.Should().BeEquivalentTo(new string[] { "online" });
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

            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.Community, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);
            limits.TextHandlers.Should().NotBeEquivalentTo(new[] { "new1", "new2" });

            limits = LicenseLimits.Create(SaaSPlans.Micro, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);
            limits.TextHandlers.Should().NotBeEquivalentTo(new[] { "new1", "new2" });

            limits = LicenseLimits.Create(SaaSPlans.SMB, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);
            limits.TextHandlers.Should().NotBeEquivalentTo(new[] { "new1", "new2" });

            limits = LicenseLimits.Create(SaaSPlans.Enterprise, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);
            limits.TextHandlers.Should().NotBeEquivalentTo(new[] { "new1", "new2" });

            limits = LicenseLimits.Create(SaaSPlans.ISV, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);
            limits.TextHandlers.Should().NotBeEquivalentTo(new[] { "new1", "new2" });

            limits = LicenseLimits.Create(SaaSPlans.ISVU, expected);
            limits.TerminalLimit.Should().NotBe(25332343);
            limits.RedistributionLimit.Should().NotBe(36523211212212);
            limits.TextHandlers.Should().NotBeEquivalentTo(new[] { "new1", "new2" });
        }
    }
}
