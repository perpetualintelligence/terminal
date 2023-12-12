/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Shared.Licensing;
using Xunit;

namespace OneImlx.Terminal.Configuration.Options
{
    public class LicensingOptionsTests
    {
        [Fact]
        public void LicensingOptionsTestsShouldHaveCorrectDefaultValues()
        {
            LicensingOptions options = new();

            options.AuthorizedApplicationId.Should().BeNull();
            options.ConsumerTenantId.Should().BeNull();
            options.LicenseKeySource.Should().Be(LicenseSources.JsonFile);
            options.LicenseKey.Should().BeNull();
            options.LicensePlan.Should().Be(TerminalLicensePlans.Demo);
            options.OnPremiseDeployment.Should().BeNull();
            options.Subject.Should().BeNull();
            options.HttpClientName.Should().BeNull();
        }
    }
}