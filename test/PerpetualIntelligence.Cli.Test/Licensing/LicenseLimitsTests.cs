/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using PerpetualIntelligence.Protocols.Licensing;
using PerpetualIntelligence.Shared.Exceptions;
using System;
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

            limits.RootCommandLimit.Should().Be(1);
            limits.CommandGroupLimit.Should().Be(5);
            limits.SubCommandLimit.Should().Be(50);
            limits.ArgumentLimit.Should().Be(500);
            limits.RedistributionLimit.Should().Be(0);

            limits.DataTypeChecks.Should().BeNull();
            limits.StrictDataType.Should().Be(false);
            limits.DefaultArguments.Should().Be(false);
            

            limits.UnicodeSupport.Should().BeEquivalentTo(new string[] { "standard" });
            limits.ErrorHandling.Should().BeEquivalentTo(new string[] { "standard" });
            limits.Stores.Should().BeEquivalentTo(new string[] { "in_memory" });
            limits.ServiceImplementations.Should().BeEquivalentTo(new string[] { "standard" });
        }

        [Fact]
        public void EnterpriseEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.Enterprise);

            limits.Plan.Should().Be(SaaSPlans.Enterprise);

            limits.RootCommandLimit.Should().Be(3);
            limits.CommandGroupLimit.Should().Be(200);
            limits.SubCommandLimit.Should().Be(2000);
            limits.ArgumentLimit.Should().Be(20000);
            limits.RedistributionLimit.Should().Be(5000);

            limits.DataTypeChecks.Should().BeEquivalentTo(new string[] { "standard", "custom" });
            limits.StrictDataType.Should().Be(true);
            limits.DefaultArguments.Should().Be(true);
            
            limits.UnicodeSupport.Should().BeEquivalentTo(new string[] { "standard" });
            limits.ErrorHandling.Should().BeEquivalentTo(new string[] { "standard", "custom" });
            limits.Stores.Should().BeEquivalentTo(new string[] { "in_memory", "json", "custom" });
            limits.ServiceImplementations.Should().BeEquivalentTo(new string[] { "standard", "custom" });
        }

        [Fact]
        public void ISVEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.ISV);

            limits.Plan.Should().Be(SaaSPlans.ISV);

            limits.RootCommandLimit.Should().Be(5);
            limits.CommandGroupLimit.Should().Be(500);
            limits.SubCommandLimit.Should().Be(5000);
            limits.ArgumentLimit.Should().Be(50000);
            limits.RedistributionLimit.Should().Be(10000);

            limits.DataTypeChecks.Should().BeEquivalentTo(new string[] { "standard", "custom" });
            limits.StrictDataType.Should().Be(true);
            limits.DefaultArguments.Should().Be(true);

            limits.UnicodeSupport.Should().BeEquivalentTo(new string[] { "standard" });
            limits.ErrorHandling.Should().BeEquivalentTo(new string[] { "standard", "custom" });
            limits.Stores.Should().BeEquivalentTo(new string[] { "in_memory", "json", "custom" });
            limits.ServiceImplementations.Should().BeEquivalentTo(new string[] { "standard", "custom" });
        }

        [Fact]
        public void ISVUEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.ISVU);

            limits.Plan.Should().Be(SaaSPlans.ISVU);

            limits.RootCommandLimit.Should().Be(null);
            limits.CommandGroupLimit.Should().Be(null);
            limits.SubCommandLimit.Should().Be(null);
            limits.ArgumentLimit.Should().Be(null);
            limits.RedistributionLimit.Should().Be(null);

            limits.DataTypeChecks.Should().BeEquivalentTo(new string[] { "standard", "custom" });
            limits.StrictDataType.Should().Be(true);
            limits.DefaultArguments.Should().Be(true);

            limits.UnicodeSupport.Should().BeEquivalentTo(new string[] { "standard" });
            limits.ErrorHandling.Should().BeEquivalentTo(new string[] { "standard", "custom" });
            limits.Stores.Should().BeEquivalentTo(new string[] { "in_memory", "json", "custom" });
            limits.ServiceImplementations.Should().BeEquivalentTo(new string[] { "standard", "custom" });
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
        public void MicroEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.Micro);

            limits.Plan.Should().Be(SaaSPlans.Micro);

            limits.RootCommandLimit.Should().Be(1);
            limits.CommandGroupLimit.Should().Be(10);
            limits.SubCommandLimit.Should().Be(100);
            limits.ArgumentLimit.Should().Be(1000);
            limits.RedistributionLimit.Should().Be(0);

            limits.DataTypeChecks.Should().BeNull();
            limits.StrictDataType.Should().Be(false);
            limits.DefaultArguments.Should().Be(false);

            limits.UnicodeSupport.Should().BeEquivalentTo(new string[] { "standard" });
            limits.ErrorHandling.Should().BeEquivalentTo(new string[] { "standard" });
            limits.Stores.Should().BeEquivalentTo(new string[] { "in_memory" });
            limits.ServiceImplementations.Should().BeEquivalentTo(new string[] { "standard" });
        }

        [Fact]
        public void SMBEdition_ShouldSetLimitsCorrectly()
        {
            LicenseLimits limits = LicenseLimits.Create(SaaSPlans.SMB);

            limits.Plan.Should().Be(SaaSPlans.SMB);

            limits.RootCommandLimit.Should().Be(1);
            limits.CommandGroupLimit.Should().Be(50);
            limits.SubCommandLimit.Should().Be(500);
            limits.ArgumentLimit.Should().Be(5000);
            limits.RedistributionLimit.Should().Be(1000);

            limits.DataTypeChecks.Should().BeEquivalentTo(new string[] { "standard" });
            limits.StrictDataType.Should().Be(true);
            limits.DefaultArguments.Should().Be(true);

            limits.UnicodeSupport.Should().BeEquivalentTo(new string[] { "standard" });
            limits.ErrorHandling.Should().BeEquivalentTo(new string[] { "standard", });
            limits.Stores.Should().BeEquivalentTo(new string[] { "in_memory", "json" });
            limits.ServiceImplementations.Should().BeEquivalentTo(new string[] { "standard" });
        }
    }
}
