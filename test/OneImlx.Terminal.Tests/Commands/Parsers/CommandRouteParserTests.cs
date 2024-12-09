///*
//    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

//    For license, terms, and data policies, go to:
//    https://terms.perpetualintelligence.com/articles/intro.html
//*/

//using FluentAssertions;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging.Abstractions;
//using OneImlx.Terminal.Commands.Handlers;
//using OneImlx.Terminal.Configuration.Options;
//using OneImlx.Terminal.Mocks;
//using OneImlx.Terminal.Runtime;
//using OneImlx.Terminal.Stores;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Xunit;

//namespace OneImlx.Terminal.Commands.Parsers
//{
//    public class CommandRouteParserTests
//    {
//        public CommandRouteParserTests()
//        {
//            terminalOptions = MockTerminalOptions.NewAliasOptions();
//            textHandler = new TerminalAsciiTextHandler();
//            commandDescriptors = new(textHandler,
//            [
//               new("root1", "root1_name", "root1_desc", CommandType.Root, CommandFlags.None),
//               new("grp1", "grp1_name", "grp1_desc", CommandType.Group, CommandFlags.None, new OwnerIdCollection("root1")),
//               new("grp2", "grp2_name", "grp2_desc", CommandType.Group, CommandFlags.None, new OwnerIdCollection("grp1")),
//               new("grp3", "grp3_name", "grp3_desc", CommandType.Group, CommandFlags.None, new OwnerIdCollection("grp2")),
//               new("cmd1", "cmd1_name", "cmd1_desc", CommandType.SubCommand, CommandFlags.None, new OwnerIdCollection("grp3")),
//               new("cmd2", "cmd2_name", "cmd2_desc", CommandType.SubCommand, CommandFlags.None, new OwnerIdCollection("grp3")),
//               new("cmd_nr1", "cmd_nr1_name", "cmd_nr1_desc", CommandType.SubCommand, CommandFlags.None),
//               new("cmd_nr2", "cmd_nr2_name", "cmd_nr2_desc", CommandType.SubCommand, CommandFlags.None)
//            ]);
//            commandStore = new TerminalInMemoryCommandStore(textHandler, commandDescriptors.Values);
//            logger = new NullLogger<TerminalRequestQueueParser>();

//            terminalRequestParser = new TerminalRequestQueueParser(textHandler, terminalOptions, logger);
//        }







//        [Theory]
//        [InlineData("root1    grp1   grp2  grp3      cmd1")]
//        [InlineData("   root1    grp1   grp2  grp3      cmd1")]
//        [InlineData("   root1    grp1   grp2  grp3      cmd1    ")]
//        [InlineData("root1    grp1   grp2  grp3      cmd1    ")]
//        public async Task Multiple_Separators_Between_Commands_Ignored(string raw)
//        {
//            terminalOptions.Parser.ParseHierarchy = true;

//            TerminalRequest request = new(Guid.NewGuid().ToString(), raw);
//            var parsedCommand = await terminalRequestParser.ParseRequestAsync(request);

//            parsedCommand.Command.Id.Should().Be("cmd1");

//            parsedCommand.Hierarchy.Should().NotBeNull();
//        }

//        [Theory]
//        [InlineData("cmd1", "cmd2")]
//        [InlineData("cmd2", "cmd1")]
//        [InlineData("cmd1", "cmd1")]
//        [InlineData("cmd2", "cmd2")]
//        public async Task Nested_SubCommand_Throws(string cmd1, string cmd2)
//        {
//            TerminalRequest request = new(Guid.NewGuid().ToString(), $"root1 grp1 grp2 grp3 {cmd1} {cmd2}");
//            Func<Task> act = async () => await terminalRequestParser.ParseRequestAsync(request);
//            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The command owner is not valid. owner={cmd1} command={cmd2}.");
//        }

//        [Theory]
//        [InlineData("root1 grp1 grp2 grp3 cmd1 cmd2", "cmd1", "cmd2")]
//        [InlineData("cmd_nr1 cmd_nr2", "cmd_nr1", "cmd_nr2")]
//        public async Task Nested_SubCommands_Throws(string cmdString, string errOwner, string errCmd)
//        {
//            TerminalRequest request = new(Guid.NewGuid().ToString(), cmdString);
//            Func<Task> act = async () => await terminalRequestParser.ParseRequestAsync(request);
//            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The command owner is not valid. owner={errOwner} command={errCmd}.");
//        }

//        [Theory]
//        [InlineData("root1", "root1")]
//        [InlineData("root1 grp1", "grp1")]
//        [InlineData("root1 grp1 grp2", "grp2")]
//        [InlineData("root1 grp1 grp2 grp3", "grp3")]
//        [InlineData("root1 grp1 grp2 grp3 cmd1", "cmd1")]
//        public async Task Options_NotSupported_Throws(string cmdStr, string errCmd)
//        {
//            Func<Task> func = async () => await terminalRequestParser.ParseRequestAsync(new TerminalRequest("id1", $"{cmdStr} --opt1 val1 -opt2 val2"));
//            await func.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support any options. command={errCmd}");
//        }

//        [Fact]
//        public async Task Root_No_Hierarchy_Parses_Correctly()
//        {
//            TerminalRequest request = new(Guid.NewGuid().ToString(), "root1");
//            var result = await terminalRequestParser.ParseRequestAsync(request);
//            result.Should().NotBeNull();
//            result.Hierarchy.Should().BeNull();

//            result.Command.Id.Should().Be("root1");
//        }

//        [Fact]
//        public async Task Root_With_Hierarchy_Parses_Correctly()
//        {
//            terminalOptions.Parser.ParseHierarchy = true;

//            TerminalRequest request = new(Guid.NewGuid().ToString(), "root1");
//            var result = await terminalRequestParser.ParseRequestAsync(request);
//            result.Should().NotBeNull();

//            result.Command.Id.Should().Be("root1");

//            result.Hierarchy.Should().NotBeNull();
//            result.Hierarchy!.ChildSubCommand.Should().BeNull();
//            result.Hierarchy.ChildGroup.Should().BeNull();
//            result.Hierarchy.LinkedCommand.Should().BeSameAs(result.Command);
//        }

//        [Theory]
//        [InlineData("@root1 grp1 grp2 grp3 cmd1 cmd2", "@root1", "grp1")]
//        [InlineData("root1 #grp1 grp2 grp3 cmd1 cmd2", "#grp1", "grp2")]
//        [InlineData("root1 grp1 $grp2 grp3 cmd1 cmd2", "$grp2", "grp3")]
//        [InlineData("root1 grp1 grp2 grp3! cmd1 cmd2", "grp3!", "cmd1")]
//        public async Task Unexpected_Inputs_Throws(string cmdStr, string errCmd, string childCmd)
//        {
//            Func<Task> func = async () => await terminalRequestParser.ParseRequestAsync(new TerminalRequest("id1", cmdStr));
//            await func.Should().ThrowAsync<TerminalException>().WithMessage($"The command owner is not valid. owner={errCmd} command={childCmd}.");
//        }

//        private readonly CommandDescriptors commandDescriptors;
//        private readonly ITerminalRequestParser terminalRequestParser;
//        private readonly ITerminalCommandStore commandStore;
//        private readonly ILogger<TerminalRequestQueueParser> logger;
//        private readonly TerminalOptions terminalOptions;
//        private readonly ITerminalTextHandler textHandler;
//    }
//}
