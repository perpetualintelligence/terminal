/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    public class AuthenticationOptionsTests
    {
        [Fact]
        public void AuthenticationOptionsTestsTestsShouldHaveCorrectDefaultValues()
        {
            AuthenticationOptions options = new();

            options.DefaultScopes.Should().BeNull();
            options.ValidHosts.Should().BeNull();
            options.HttpClientName.Should().BeNull();
            options.UserFlow.Should().BeNull();
        }
    }
}