/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Configuration.Options
{
    public class DebugOptionsTests
    {
        [Fact]
        public void DebugOptions_HasCorrect_DefaultValues()
        {
            DebugOptions options = new();
            options.ReturnRouted.Should().BeNull();
        }
    }
}
