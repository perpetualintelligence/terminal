/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Configuration.Options
{
    public class AuthenticationOptionsTests
    {
        [Fact]
        public void AuthenticationOptions_HasCorrect_DefaultValues()
        {
            AuthenticationOptions options = new();

            options.DefaultScopes.Should().BeNull();
            options.ValidHosts.Should().BeNull();
            options.UserFlow.Should().BeNull();
        }
    }
}
