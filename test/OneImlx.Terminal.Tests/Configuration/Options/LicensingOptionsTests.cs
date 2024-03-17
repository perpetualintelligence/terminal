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

            options.Application.Should().BeNull();
            options.TenantId.Should().BeNull();
            options.LicenseKey.Should().BeNull();
            options.LicensePlan.Should().Be(TerminalLicensePlans.Demo);
            options.Deployment.Should().BeNull();
            options.Id.Should().BeNull();
            options.HttpClientName.Should().BeNull();
        }
    }
}