/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Configuration.Options
{
    public class CheckerOptionsTests
    {
        public CheckerOptionsTests()
        {
        }

        [Fact]
        public void CheckerOptionsShouldHaveCorrectDefaultValues()
        {
            CheckerOptions options = new();

            options.AllowObsolete.Should().BeFalse();
            options.StrictValueType.Should().BeFalse();
        }
    }
}