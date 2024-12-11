/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Stores;
using OneImlx.Test.FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Runtime
{
    public class TerminalRequestQueueUtf8ParserTests
    {
        public TerminalRequestQueueUtf8ParserTests()
        {
            terminalTextHandler = new TerminalUtf8TextHandler();
            mockCommandStore = new Mock<ITerminalCommandStore>();
            mockLogger = new Mock<ILogger<TerminalRequestQueueParser>>();
            terminalOptions = Options.Create<TerminalOptions>(new TerminalOptions());

            parser = new TerminalRequestQueueParser(terminalTextHandler, terminalOptions, mockLogger.Object);
        }

        [Fact]
        public async Task Delimited_Argument_Non_Ending_Throws()
        {
            Func<Task> act = async () => { await parser.ParseRequestAsync(new TerminalRequest("पहचान", "मूल1 समूह1 समूह2 आदेश1 तर्क1 \"तर्क मान समापन नहीं")); };
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_argument")
                .WithErrorDescription("The argument value is missing the closing delimiter. argument=\"तर्क मान समापन नहीं");
        }

        [Fact]
        public async Task Delimited_Option_Non_Ending_Throws()
        {
            Func<Task> act = async () => { await parser.ParseRequestAsync(new TerminalRequest("पहचान", "मूल1 समूह1 समूह2 आदेश1 तर्क1 -विकल्प1 \"विकल्प मान समापन नहीं")); };
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_option")
                .WithErrorDescription("The option value is missing the closing delimiter. option=विकल्प1");
        }

        [Fact]
        public async Task Empty_Raw_Does_Not_Throws()
        {
            TerminalParsedRequest parsed = await parser.ParseRequestAsync(new TerminalRequest("पहचान1", ""));
            parsed.Tokens.Should().BeEmpty();
            parsed.Options.Should().BeEmpty();

            parsed = await parser.ParseRequestAsync(new TerminalRequest("पहचान1", "    "));
            parsed.Tokens.Should().BeEmpty();
            parsed.Options.Should().BeEmpty();
        }

        [Fact]
        public async Task Null_or_Empty_Id_Throws()
        {
            Func<Task> act = async () => { await parser.ParseRequestAsync(new TerminalRequest("", "मूल1 समूह1 आदेश1 तर्क1 तर्क2")); };
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("id");

            act = async () => { await parser.ParseRequestAsync(new TerminalRequest(null!, "मूल1 समूह1 आदेश1 तर्क1 तर्क2")); };
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("id");

            act = async () => { await parser.ParseRequestAsync(new TerminalRequest("   ", "मूल1 समूह1 आदेश1 तर्क1 तर्क2")); };
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("id");
        }

        [Fact]
        public async Task Null_Raw_Throws()
        {
            Func<Task> act = async () => { await parser.ParseRequestAsync(new TerminalRequest("पहचान1", null!)); };
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("raw");
        }

        [Fact]
        public async Task Only_Options_Does_Not_Error()
        {
            TerminalParsedRequest parsedOutput = await parser.ParseRequestAsync(new TerminalRequest("पहचान1", "-विकल्प1 मान1 --विकल्प2 मान2 -विकल्प3"));
            parsedOutput.Tokens.Should().BeEmpty();
            parsedOutput.Options.Should().HaveCount(3);
            parsedOutput.Options!["विकल्प1"].Should().Be(new("मान1", true));
            parsedOutput.Options["विकल्प2"].Should().Be(new("मान2", false));
            parsedOutput.Options["विकल्प3"].Should().Be(new(true.ToString(), true));
        }

        [Fact]
        public async Task Parses_All_Delimited_Options_Values()
        {
            TerminalParsedRequest parsedOutput = await parser.ParseRequestAsync(new TerminalRequest("पहचान1", "मूल1 समूह1 आदेश1 तर्क1 तर्क2 -विकल्प1 \"मान1\" --विकल्प2 \"मान2\" --विकल्प3 \"सीमांकित मान3\" -विकल्प4 \"सीमांकित मान4\""));
            parsedOutput.Tokens.Should().BeEquivalentTo(["मूल1", "समूह1", "आदेश1", "तर्क1", "तर्क2"]);
            parsedOutput.Options.Should().HaveCount(4);
            parsedOutput.Options!["विकल्प1"].Should().Be(("मान1", true));
            parsedOutput.Options["विकल्प2"].Should().Be(("मान2", false));
            parsedOutput.Options["विकल्प3"].Should().Be(("सीमांकित मान3", false));
            parsedOutput.Options["विकल्प4"].Should().Be(("सीमांकित मान4", true));
        }

        [Fact]
        public async Task Parses_Delimited_Arguments()
        {
            TerminalParsedRequest parsedOutput = await parser.ParseRequestAsync(new TerminalRequest("पहचान1", "मूल1 समूह1 आदेश1 \"तर्क1\" \"तर्क2\" तर्क3"));
            parsedOutput.Tokens.Should().BeEquivalentTo(["मूल1", "समूह1", "आदेश1", "तर्क1", "तर्क2", "तर्क3"]);
        }

        [Fact]
        public async Task Parses_Ending_With_No_Value_Option()
        {
            TerminalParsedRequest parsedOutput = await parser.ParseRequestAsync(new TerminalRequest("पहचान1", "मूल1 समूह1 आदेश1 तर्क1 तर्क2 तर्क3 तर्क4 तर्क5 तर्क6 तर्क7 तर्क8 तर्क9 तर्क10 -विकल्प1 मान1 --विकल्प2 मान2 -विकल्प3 -विकल्प4 36.69 --विकल्प5 \"सीमांकित मान\" -विकल्प6"));
            parsedOutput.Tokens.Should().BeEquivalentTo(["मूल1", "समूह1", "आदेश1", "तर्क1", "तर्क2", "तर्क3", "तर्क4", "तर्क5", "तर्क6", "तर्क7", "तर्क8", "तर्क9", "तर्क10"]);
            parsedOutput.Options.Should().HaveCount(6);
            parsedOutput.Options!["विकल्प1"].Should().Be(("मान1", true));
            parsedOutput.Options["विकल्प2"].Should().Be(("मान2", false));
            parsedOutput.Options["विकल्प3"].Should().Be((true.ToString(), true));
            parsedOutput.Options["विकल्प4"].Should().Be(("36.69", true));
            parsedOutput.Options["विकल्प5"].Should().Be(("सीमांकित मान", false));
            parsedOutput.Options["विकल्प6"].Should().Be((true.ToString(), true));
        }

        [Fact]
        public async Task Parses_Full_Correctly()
        {
            TerminalParsedRequest parsedOutput = await parser.ParseRequestAsync(new TerminalRequest("पहचान1", "मूल1 समूह1 आदेश1 तर्क1 तर्क2 तर्क3 तर्क4 तर्क5 तर्क6 तर्क7 तर्क8 तर्क9 तर्क10 -विकल्प1 मान1 --विकल्प2 मान2 -विकल्प3 -विकल्प4 36.69 --विकल्प5 \"सीमांकित मान\""));
            parsedOutput.Tokens.Should().BeEquivalentTo(["मूल1", "समूह1", "आदेश1", "तर्क1", "तर्क2", "तर्क3", "तर्क4", "तर्क5", "तर्क6", "तर्क7", "तर्क8", "तर्क9", "तर्क10"]);
            parsedOutput.Options.Should().HaveCount(5);
            parsedOutput.Options!["विकल्प1"].Should().Be(("मान1", true));
            parsedOutput.Options["विकल्प2"].Should().Be(("मान2", false));
            parsedOutput.Options["विकल्प3"].Should().Be((true.ToString(), true));
            parsedOutput.Options["विकल्प4"].Should().Be(("36.69", true));
            parsedOutput.Options["विकल्प5"].Should().Be(("सीमांकित मान", false));
        }

        [Theory]
        [InlineData("मूल1", new string[] { "मूल1" })]
        [InlineData("मूल1 समूह1 समूह2 समूह3 समूह4 समूह5 समूह6 समूह7 समूह8 समूह9 समूह10", new string[] { "मूल1", "समूह1", "समूह2", "समूह3", "समूह4", "समूह5", "समूह6", "समूह7", "समूह8", "समूह9", "समूह10" })]
        [InlineData("मूल1 समूह1 समूह2 समूह3 आदेश1", new string[] { "मूल1", "समूह1", "समूह2", "समूह3", "आदेश1" })]
        [InlineData("मूल1 समूह1 आदेश1", new string[] { "मूल1", "समूह1", "आदेश1" })]
        [InlineData("मूल1 समूह1 आदेश1 तर्क1 तर्क2 तर्क3 तर्क4 तर्क5 तर्क6 तर्क7 तर्क8 तर्क9 तर्क10", new string[] { "मूल1", "समूह1", "आदेश1", "तर्क1", "तर्क2", "तर्क3", "तर्क4", "तर्क5", "तर्क6", "तर्क7", "तर्क8", "तर्क9", "तर्क10" })]
        public async Task Parses_Tokens_Correctly(string raw, string[] tokens)
        {
            TerminalParsedRequest parsedOutput = await parser.ParseRequestAsync(new TerminalRequest("पहचान1", raw));
            parsedOutput.Tokens.Should().BeEquivalentTo(tokens);
            parsedOutput.Options.Should().BeEmpty();
        }

        [Fact]
        public async Task Parses_With_Unicode_Separators_Text_Correctly()
        {
            // Arrange
            char separator = 'あ';
            char valueSeparator = 'う';

            terminalOptions.Value.Parser.Separator = separator;
            terminalOptions.Value.Parser.OptionValueSeparator = valueSeparator;

            var request = new TerminalRequest
            (
                "पहचान1",
                $"मूल1{separator}समूह1{separator}आदेश1{separator}तर्क1{separator}तर्क2{separator}तर्क3{separator}तर्क4{separator}तर्क5{separator}तर्क6{separator}तर्क7{separator}तर्क8{separator}तर्क9{separator}तर्क10{separator}" +
                $"-विकल्प1{valueSeparator}मान1{separator}--विकल्प2{valueSeparator}मान2{separator}-विकल्प3{separator}-विकल्प4{valueSeparator}36.69{separator}--विकल्प5{valueSeparator}\"सीमांकित मान\""
            );

            // Act
            TerminalParsedRequest parsedOutput = await parser.ParseRequestAsync(request);

            // Assert
            parsedOutput.Tokens.Should().BeEquivalentTo(["मूल1", "समूह1", "आदेश1", "तर्क1", "तर्क2", "तर्क3", "तर्क4", "तर्क5", "तर्क6", "तर्क7", "तर्क8", "तर्क9", "तर्क10"]);
            parsedOutput.Options.Should().HaveCount(5);
            parsedOutput.Options!["विकल्प1"].Should().Be(("मान1", true));
            parsedOutput.Options["विकल्प2"].Should().Be(("मान2", false));
            parsedOutput.Options["विकल्प3"].Should().Be((true.ToString(), true));
            parsedOutput.Options["विकल्प4"].Should().Be(("36.69", true));
            parsedOutput.Options["विकल्प5"].Should().Be(("सीमांकित मान", false));
        }

        private readonly Mock<ITerminalCommandStore> mockCommandStore;
        private readonly Mock<ILogger<TerminalRequestQueueParser>> mockLogger;
        private readonly TerminalRequestQueueParser parser;
        private readonly IOptions<TerminalOptions> terminalOptions;
        private readonly ITerminalTextHandler terminalTextHandler;
    }
}
