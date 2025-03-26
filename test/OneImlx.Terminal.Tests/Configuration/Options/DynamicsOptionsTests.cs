/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Configuration.Options
{
    public class DynamicsOptionsTests
    {
        [Fact]
        public void HasCorrect_DefaultValues()
        {
            DynamicsOptions options = new();

            options.Enabled.Should().BeFalse();
        }
    }
}
