/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Shared
{
    public class TerminalIdentifiersTests
    {
        [Fact]
        public void TerminalIdentifiers_Defines_Identifiers()
        {
            typeof(TerminalIdentifiers).Should().HaveConstantCount(10);

            TerminalIdentifiers.OfflineLicenseMode.Should().Be("offline");
            TerminalIdentifiers.IsolatedDeployment.Should().Be("isolated");
            TerminalIdentifiers.OnPremiseDeployment.Should().Be("onpremise");
            TerminalIdentifiers.CustomHandler.Should().Be("custom");
            TerminalIdentifiers.DefaultHandler.Should().Be("default");
            TerminalIdentifiers.TestApplicationId.Should().Be("08c6925f-a734-4e24-8d84-e06737420766");
            TerminalIdentifiers.StreamDelimiter.Should().Be(0x1E);
            TerminalIdentifiers.SenderIdToken.Should().Be("sender_id");
            TerminalIdentifiers.SenderEndpointToken.Should().Be("sender_endpoint");
            TerminalIdentifiers.SpaceSeparator.Should().Be(' ');
        }
    }
}
