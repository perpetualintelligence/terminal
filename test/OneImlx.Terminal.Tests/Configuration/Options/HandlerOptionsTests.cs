/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Configuration.Options
{
    public class HandlerOptionsTests
    {
        [Fact]
        public void HostingOptionsShouldHaveCorrectDefaultValues()
        {
            HandlerOptions options = new();

            options.LicenseHandler.Should().Be(TerminalHandlers.OnlineLicenseHandler);
            options.ServiceHandler.Should().Be(TerminalHandlers.DefaultHandler);
            options.StoreHandler.Should().Be(TerminalHandlers.InMemoryHandler);
        }
    }
}