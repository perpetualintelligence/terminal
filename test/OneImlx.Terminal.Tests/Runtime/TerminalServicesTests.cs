/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

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
                    CommandDelimiter = ";", // Delimiter to separate commands within a batch
                    BatchDelimiter = "|"   // Delimiter to mark the end of a complete batch
                }
            };
        }

        [Fact]
        public void CreateBatch_WithCustomDelimiters_ShouldJoinCommandsAndEndWithMessageDelimiter()
        {
            var commands = new[] { "cmd1", "cmd2", "cmd3" };
            var result = TerminalServices.CreateBatch(";", "|", commands);
            result.Should().Be("cmd1;cmd2;cmd3|");
        }

        [Fact]
        public void CreateBatch_WithEmptyCommands_ShouldReturnOnlyMessageDelimiter()
        {
            var commands = System.Array.Empty<string>();
            var result = TerminalServices.CreateBatch(";", "|", commands);
            result.Should().Be("|");
        }

        [Fact]
        public void CreateBatch_WithOptions_ShouldJoinCommandsAndEndWithMessageDelimiter()
        {
            var commands = new[] { "cmd1", "cmd2", "cmd3" };
            var result = TerminalServices.CreateBatch(_terminalOptions, commands);
            result.Should().Be("cmd1;cmd2;cmd3|");
        }

        [Fact]
        public void CreateBatch_WithSingleCommand_ShouldAppendMessageDelimiter()
        {
            var commands = new[] { "cmd1" };
            var result = TerminalServices.CreateBatch(";", "|", commands);
            result.Should().Be("cmd1|");
        }

        // Tests for Encode and Decode License Contents

        [Theory]
        [InlineData("dGVzdCBsaWNlbnNlY29uZGVudHM=")]
        [InlineData("[{\"prop1\": \"value1\", \"prop2\": 42, \"prop3\": [\"string\", 3.14, true, {\"nested\": \"object\"}]}]")]
        public void EncodeLicenseContents_ShouldReturn_ExpectedString(string licenseContents)
        {
            // Encode
            string result = TerminalServices.EncodeLicenseContents(licenseContents);
            result.Should().NotBe(licenseContents);

            // Decode
            string decodedContents = TerminalServices.DecodeLicenseContents(result);
            decodedContents.Should().Be(licenseContents);
        }

        private readonly TerminalOptions _terminalOptions;
    }
}
