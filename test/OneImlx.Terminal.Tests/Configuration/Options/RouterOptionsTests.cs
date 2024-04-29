/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Configuration.Options
{
    public class RouterOptionsTests
    {
        [Fact]
        public void RouterOptionsShouldHaveCorrectDefaultValues()
        {
            RouterOptions options = new();

            options.Caret.Should().Be(">");
            options.Timeout.Should().Be(25000);
            options.RemoteMessageMaxLength.Should().Be(1024);
            options.MaxRemoteClients.Should().Be(5);
            options.EnableRemoteDelimiters.Should().BeNull();
            options.RemoteMessageDelimiter.Should().Be("$m$");
            options.RemoteCommandDelimiter.Should().Be("$c$");
        }
    }
}
