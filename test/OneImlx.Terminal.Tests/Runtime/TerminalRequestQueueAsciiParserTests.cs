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
    public class TerminalRequestQueueAsciiParserTests
    {
        public TerminalRequestQueueAsciiParserTests()
        {
            terminalTextHandler = new TerminalAsciiTextHandler();
            mockCommandStore = new Mock<ITerminalCommandStore>();
            mockLogger = new Mock<ILogger<TerminalRequestQueueParser>>();
            terminalOptions = Microsoft.Extensions.Options.Options.Create<TerminalOptions>(new TerminalOptions());

            parser = new TerminalRequestQueueParser(terminalTextHandler, terminalOptions, mockLogger.Object);
        }

        [Fact]
        public async Task Delimited_Argument_Non_Ending_Throws()
        {
            Func<Task> act = async () => { await parser.ParseRequestAsync(new TerminalRequest("id", "root1 grp1 grp2 cmd1 arg1 \"arg value not delimited")); };
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_argument")
                .WithErrorDescription("The argument value is missing the closing delimiter. argument=\"arg value not delimited");
        }

        [Fact]
        public async Task Delimited_Option_Non_Ending_Throws()
        {
            Func<Task> act = async () => { await parser.ParseRequestAsync(new TerminalRequest("id", "root1 grp1 grp2 cmd1 arg1 -opt1 \"opt value not delimited")); };
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_option")
                .WithErrorDescription("The option value is missing the closing delimiter. option=opt1");
        }

        [Fact]
        public async Task Empty_Raw_Does_Not_Throws()
        {
            TerminalParsedRequest parsed = await parser.ParseRequestAsync(new TerminalRequest("id1", ""));
            parsed.Tokens.Should().BeEmpty();
            parsed.Options.Should().BeEmpty();

            parsed = await parser.ParseRequestAsync(new TerminalRequest("id1", "    "));
            parsed.Tokens.Should().BeEmpty();
            parsed.Options.Should().BeEmpty();
        }

        [Fact]
        public async Task Multiple_Separators_Are_Ignored()
        {
            TerminalRequest request = new("id1", "    root1    grp1   grp2  grp3      cmd1    arg1  arg2    --opt1     val1  --opt2 val2      -opt3        ");
            var parsedCommand = await parser.ParseRequestAsync(request);

            parsedCommand.Tokens.Should().BeEquivalentTo(["root1", "grp1", "grp2", "grp3", "cmd1", "arg1", "arg2"]);

            parsedCommand.Options.Should().HaveCount(3);
            parsedCommand.Options!["opt1"].Should().Be(new("val1", false));
            parsedCommand.Options["opt2"].Should().Be(new("val2", false));
            parsedCommand.Options["opt3"].Should().Be(new(true.ToString(), true));
        }

        [Fact]
        public async Task Null_or_Empty_Id_Throws()
        {
            Func<Task> act = async () => { await parser.ParseRequestAsync(new TerminalRequest("", "root1 grp1 cmd1 arg1 arg2")); };
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("id");

            act = async () => { await parser.ParseRequestAsync(new TerminalRequest(null!, "root1 grp1 cmd1 arg1 arg2")); };
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("id");

            act = async () => { await parser.ParseRequestAsync(new TerminalRequest("   ", "root1 grp1 cmd1 arg1 arg2")); };
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("id");
        }

        [Fact]
        public async Task Null_Raw_Throws()
        {
            Func<Task> act = async () => { await parser.ParseRequestAsync(new TerminalRequest("id1", null!)); };
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("raw");
        }

        [Fact]
        public async Task Only_Options_Does_Not_Error()
        {
            TerminalParsedRequest parsedOutput = await parser.ParseRequestAsync(new TerminalRequest("id1", "-opt1 val1 --opt2 val2 -opt3"));
            parsedOutput.Tokens.Should().BeEmpty();
            parsedOutput.Options.Should().HaveCount(3);
            parsedOutput.Options!["opt1"].Should().Be(("val1", true));
            parsedOutput.Options["opt2"].Should().Be(("val2", false));
            parsedOutput.Options["opt3"].Should().Be((true.ToString(), true));
        }

        [Fact]
        public async Task Parses_All_Delimited_Options_Values()
        {
            TerminalParsedRequest parsedOutput = await parser.ParseRequestAsync(new TerminalRequest("id1", "root1 grp1 cmd1 arg1 arg2 -opt1 \"val1\" --opt2 \"val2\" --opt3 \"delimited val3\" -opt4 \"    delimited val4     \""));
            parsedOutput.Tokens.Should().BeEquivalentTo(["root1", "grp1", "cmd1", "arg1", "arg2"]);
            parsedOutput.Options.Should().HaveCount(4);
            parsedOutput.Options!["opt1"].Should().Be(("val1", true));
            parsedOutput.Options["opt2"].Should().Be(("val2", false));
            parsedOutput.Options["opt3"].Should().Be(("delimited val3", false));
            parsedOutput.Options["opt4"].Should().Be(("    delimited val4     ", true));
        }

        [Fact]
        public async Task Parses_Delimited_Arguments()
        {
            TerminalParsedRequest parsedOutput = await parser.ParseRequestAsync(new TerminalRequest("id1", "root1 grp1 cmd1 \"arg1\" \"  arg2    \" arg3"));
            parsedOutput.Tokens.Should().BeEquivalentTo(["root1", "grp1", "cmd1", "arg1", "  arg2    ", "arg3"]);
        }

        [Fact]
        public async Task Parses_Delimited_Commands()
        {
            TerminalParsedRequest parsedOutput = await parser.ParseRequestAsync(new TerminalRequest("id1", "\"root1\" \"grp1\" \"cmd1\" \"arg1\" \"  arg2    \" arg3"));
            parsedOutput.Tokens.Should().BeEquivalentTo(["root1", "grp1", "cmd1", "arg1", "  arg2    ", "arg3"]);
        }

        [Fact]
        public async Task Parses_Ending_With_No_Value_Option()
        {
            TerminalParsedRequest parsedOutput = await parser.ParseRequestAsync(new TerminalRequest("id1", "root1 grp1 cmd1 arg1 arg2 arg3 arg4 arg5 arg6 arg7 arg8 arg9 arg10 -opt1 val1 --opt2 val2 -opt3 -opt4 36.69 --opt5 \"delimited val\" -opt6"));
            parsedOutput.Tokens.Should().BeEquivalentTo(["root1", "grp1", "cmd1", "arg1", "arg2", "arg3", "arg4", "arg5", "arg6", "arg7", "arg8", "arg9", "arg10"]);
            parsedOutput.Options.Should().HaveCount(6);
            parsedOutput.Options!["opt1"].Should().Be(("val1", true));
            parsedOutput.Options["opt2"].Should().Be(("val2", false));
            parsedOutput.Options["opt3"].Should().Be((true.ToString(), true));
            parsedOutput.Options["opt4"].Should().Be(("36.69", true));
            parsedOutput.Options["opt5"].Should().Be(("delimited val", false));
            parsedOutput.Options["opt6"].Should().Be((true.ToString(), true));
        }

        [Fact]
        public async Task Parses_Full_With_Distinct_Separator_Correctly()
        {
            terminalOptions.Value.Parser.OptionValueSeparator = '=';

            TerminalParsedRequest parsedOutput = await parser.ParseRequestAsync(new TerminalRequest("id1", "root1 grp1 cmd1 arg1 arg2 arg3 arg4 arg5 arg6 arg7 arg8 arg9 arg10 -opt1=val1 --opt2=val2 -opt3 -opt4=36.69 --opt5=\"delimited val\""));
            parsedOutput.Tokens.Should().BeEquivalentTo(["root1", "grp1", "cmd1", "arg1", "arg2", "arg3", "arg4", "arg5", "arg6", "arg7", "arg8", "arg9", "arg10"]);
            parsedOutput.Options.Should().HaveCount(5);
            parsedOutput.Options!["opt1"].Should().Be(("val1", true));
            parsedOutput.Options["opt2"].Should().Be(("val2", false));
            parsedOutput.Options["opt3"].Should().Be((true.ToString(), true));
            parsedOutput.Options["opt4"].Should().Be(("36.69", true));
            parsedOutput.Options["opt5"].Should().Be(("delimited val", false));
        }

        [Fact]
        public async Task Parses_Full_With_Same_Separator_Correctly()
        {
            TerminalParsedRequest parsedOutput = await parser.ParseRequestAsync(new TerminalRequest("id1", "root1 grp1 cmd1 arg1 arg2 arg3 arg4 arg5 arg6 arg7 arg8 arg9 arg10 -opt1 val1 --opt2 val2 -opt3 -opt4 36.69 --opt5 \"delimited val\""));
            parsedOutput.Tokens.Should().BeEquivalentTo(["root1", "grp1", "cmd1", "arg1", "arg2", "arg3", "arg4", "arg5", "arg6", "arg7", "arg8", "arg9", "arg10"]);
            parsedOutput.Options.Should().HaveCount(5);
            parsedOutput.Options!["opt1"].Should().Be(("val1", true));
            parsedOutput.Options["opt2"].Should().Be(("val2", false));
            parsedOutput.Options["opt3"].Should().Be((true.ToString(), true));
            parsedOutput.Options["opt4"].Should().Be(("36.69", true));
            parsedOutput.Options["opt5"].Should().Be(("delimited val", false));
        }

        [Theory]
        [InlineData("root1", new string[] { "root1" })]
        [InlineData("root1 grp1 grp2 grp3 grp4 grp5 grp6 grp7 grp8 grp9 grp10", new string[] { "root1", "grp1", "grp2", "grp3", "grp4", "grp5", "grp6", "grp7", "grp8", "grp9", "grp10" })]
        [InlineData("root1 grp1 grp2 grp3 cmd1", new string[] { "root1", "grp1", "grp2", "grp3", "cmd1" })]
        [InlineData("root1 grp1 cmd1", new string[] { "root1", "grp1", "cmd1" })]
        [InlineData("root1 grp1 cmd1 arg1 arg2 arg3 arg4 arg5 arg6 arg7 arg8 arg9 arg10", new string[] { "root1", "grp1", "cmd1", "arg1", "arg2", "arg3", "arg4", "arg5", "arg6", "arg7", "arg8", "arg9", "arg10" })]
        public async Task Parses_Tokens_Correctly(string raw, string[] tokens)
        {
            TerminalParsedRequest parsedOutput = await parser.ParseRequestAsync(new TerminalRequest("id1", raw));
            parsedOutput.Tokens.Should().BeEquivalentTo(tokens);
            parsedOutput.Options.Should().BeEmpty();
        }

        private readonly Mock<ITerminalCommandStore> mockCommandStore;
        private readonly Mock<ILogger<TerminalRequestQueueParser>> mockLogger;
        private readonly TerminalRequestQueueParser parser;
        private readonly IOptions<TerminalOptions> terminalOptions;
        private readonly ITerminalTextHandler terminalTextHandler;
    }
}
