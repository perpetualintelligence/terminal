/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using Xunit;

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    public class RouterOptionsTests : InitializerTests
    {
        public RouterOptionsTests() : base(TestLogger.Create<RouterOptionsTests>())
        {
        }

        [Fact]
        public void RouterOptionsShouldHaveCorrectDefaultValues()
        {
            RouterOptions options = new();

            options.Caret.Should().Be(">");
            options.Timeout.Should().Be(25000);
            options.SyncDelay.Should().Be(100);
            options.MaxCommandStringLength.Should().Be(1024);
            options.RemoteMaxClients.Should().Be(5);
        }
    }
}