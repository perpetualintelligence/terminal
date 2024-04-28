/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Terminal.Configuration.Options;
using Xunit;

namespace OneImlx.Terminal.Runtime
{
    public class TerminalServicesTests
    {
        public TerminalServicesTests()
        {
            _terminalOptions = new TerminalOptions();
            _terminalOptions.Router.RemoteMessageDelimiter = "|";
        }

        [Fact]
        public void DelimitedCommandString_ShouldAddDelimiterToElement_NotEndingWithDelimiter()
        {
            var input = new[] { "cmd1|", "cmd2", "cmd3" };

            var result = TerminalServices.DelimitedMessage(_terminalOptions, input);
            result.Should().Be("cmd1|cmd2|cmd3|");

            result = TerminalServices.DelimitedMessage("|", input);
            result.Should().Be("cmd1|cmd2|cmd3|");
        }

        [Fact]
        public void DelimitedCommandString_ShouldReturn_EmptyString_ForEmptyInput()
        {
            var input = System.Array.Empty<string>();

            var result = TerminalServices.DelimitedMessage(_terminalOptions, input);
            result.Should().BeEmpty();

            result = TerminalServices.DelimitedMessage("|", input);
            result.Should().BeEmpty();
        }

        [Fact]
        public void DelimitedCommandString_ShouldReturn_ExpectedString_ForMultipleInputs()
        {
            var input = new[] { "cmd1", "cmd2", "cmd3" };

            var result = TerminalServices.DelimitedMessage(_terminalOptions, input);
            result.Should().Be("cmd1|cmd2|cmd3|");

            result = TerminalServices.DelimitedMessage("|", input);
            result.Should().Be("cmd1|cmd2|cmd3|");
        }

        [Fact]
        public void DelimitedCommandString_ShouldReturn_ExpectedString_ForSingleInput()
        {
            var input = new[] { "cmd1" };
            var result = TerminalServices.DelimitedMessage(_terminalOptions, input);
            result.Should().Be("cmd1|");

            result = TerminalServices.DelimitedMessage("|", input);
            result.Should().Be("cmd1|");
        }

        private TerminalOptions _terminalOptions;
    }
}
