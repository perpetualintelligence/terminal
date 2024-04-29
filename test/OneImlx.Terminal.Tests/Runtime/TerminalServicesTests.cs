/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Terminal.Configuration.Options;
using Xunit;

namespace OneImlx.Terminal.Runtime.Tests
{
    public class TerminalServicesTests
    {
        public TerminalServicesTests()
        {
            _terminalOptions = new TerminalOptions
            {
                Router = new RouterOptions
                {
                    RemoteCommandDelimiter = ";", // Delimiter to separate commands within a message
                    RemoteMessageDelimiter = "|"  // Delimiter to mark the end of a complete message
                }
            };
        }

        [Fact]
        public void DelimitedMessage_WithCustomDelimiters_ShouldAddDelimiterToElements_NotEndingWithCommandDelimiter()
        {
            var commands = new[] { "cmd1;", "cmd2", "cmd3" };
            var result = TerminalServices.DelimitedMessage(";", "|", commands);
            result.Should().Be("cmd1;cmd2;cmd3;|");
        }

        [Fact]
        public void DelimitedMessage_WithCustomDelimiters_ShouldReturn_EmptyString_ForEmptyInput()
        {
            var commands = System.Array.Empty<string>();
            var result = TerminalServices.DelimitedMessage(";", "|", commands);
            result.Should().Be("|");
        }

        [Fact]
        public void DelimitedMessage_WithCustomDelimiters_ShouldReturn_ExpectedString_ForMultipleInputs()
        {
            var commands = new[] { "cmd1", "cmd2", "cmd3" };
            var result = TerminalServices.DelimitedMessage(";", "|", commands);
            result.Should().Be("cmd1;cmd2;cmd3;|");
        }

        [Fact]
        public void DelimitedMessage_WithCustomDelimiters_ShouldReturn_ExpectedString_ForSingleInput()
        {
            var commands = new[] { "cmd1" };
            var result = TerminalServices.DelimitedMessage(";", "|", commands);
            result.Should().Be("cmd1;|");
        }

        [Fact]
        public void DelimitedMessage_WithOptions_ShouldAddDelimiterToElements_NotEndingWithCommandDelimiter()
        {
            var commands = new[] { "cmd1;", "cmd2", "cmd3" };
            var result = TerminalServices.DelimitedMessage(_terminalOptions, commands);
            result.Should().Be("cmd1;cmd2;cmd3;|");
        }

        [Fact]
        public void DelimitedMessage_WithOptions_ShouldReturn_EmptyString_ForEmptyInput()
        {
            var commands = System.Array.Empty<string>();
            var result = TerminalServices.DelimitedMessage(_terminalOptions, commands);
            result.Should().Be("|");
        }

        [Fact]
        public void DelimitedMessage_WithOptions_ShouldReturn_ExpectedString_ForMultipleInputs()
        {
            var commands = new[] { "cmd1", "cmd2", "cmd3" };
            var result = TerminalServices.DelimitedMessage(_terminalOptions, commands);
            result.Should().Be("cmd1;cmd2;cmd3;|");
        }

        [Fact]
        public void DelimitedMessage_WithOptions_ShouldReturn_ExpectedString_ForSingleInput()
        {
            var commands = new[] { "cmd1" };
            var result = TerminalServices.DelimitedMessage(_terminalOptions, commands);
            result.Should().Be("cmd1;|");
        }

        private readonly TerminalOptions _terminalOptions;
    }
}
