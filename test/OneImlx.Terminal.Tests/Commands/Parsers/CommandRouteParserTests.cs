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



//        [Fact]
//        public async Task CommandRoute_Is_Set_In_Result()
//        {
//            TerminalRequest request = new(Guid.NewGuid().ToString(), "root1");
//            var result = await terminalRequestParser.ParseRequestAsync(request);
//            result.Should().NotBeNull();

//            result.CommandRoute.Should().NotBeNull();
//            result.CommandRoute.Should().BeSameAs(request);
//        }

//        [Theory]
//        [InlineData(false)]
//        [InlineData(null)]
//        public async Task Does_Not_Parse_Hierarchy_If_Option_Not_Set(bool? parseHierarchy)
//        {
//            terminalOptions.Parser.ParseHierarchy = parseHierarchy;

//            TerminalRequest request = new(Guid.NewGuid().ToString(), "root1");
//            var parsedCommand = await terminalRequestParser.ParseRequestAsync(request);
//            parsedCommand.Hierarchy.Should().BeNull();

//            request = new(Guid.NewGuid().ToString(), "root1 grp1");
//            parsedCommand = await terminalRequestParser.ParseRequestAsync(request);
//            parsedCommand.Hierarchy.Should().BeNull();

//            request = new(Guid.NewGuid().ToString(), "root1 grp1 grp2");
//            parsedCommand = await terminalRequestParser.ParseRequestAsync(request);
//            parsedCommand.Hierarchy.Should().BeNull();

//            request = new(Guid.NewGuid().ToString(), "root1 grp1 grp2 grp3");
//            parsedCommand = await terminalRequestParser.ParseRequestAsync(request);
//            parsedCommand.Hierarchy.Should().BeNull();

//            request = new(Guid.NewGuid().ToString(), "root1 grp1 grp2 grp3 cmd1");
//            parsedCommand = await terminalRequestParser.ParseRequestAsync(request);
//            parsedCommand.Hierarchy.Should().BeNull();
//        }

//        [Theory]
//        [InlineData("root1 grp1 grp2 grp3 cmd1 cmd1", "cmd1")]
//        [InlineData("root1 grp1 grp2 grp3 grp3", "grp3")]
//        [InlineData("root1 grp1 grp2 grp2", "grp2")]
//        [InlineData("root1 grp1 grp1", "grp1")]
//        [InlineData("root1 root1", "root1")]
//        public async Task Duplicate_Commands_Throws(string cmdString, string duplicateCmd)
//        {
//            TerminalRequest request = new(Guid.NewGuid().ToString(), cmdString);
//            Func<Task> act = async () => await terminalRequestParser.ParseRequestAsync(request);
//            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The command owner is not valid. owner={duplicateCmd} command={duplicateCmd}.");
//        }

//        [Fact]
//        public async Task Group_InvalidNestedRoot_Throws()
//        {
//            TerminalRequest request = new(Guid.NewGuid().ToString(), "root1 grp1 invalid_grp2 grp3");
//            Func<Task> act = async () => await terminalRequestParser.ParseRequestAsync(request);
//            await act.Should().ThrowAsync<TerminalException>().WithMessage("The command owner is not valid. owner=invalid_grp2 command=grp3.");
//        }

//        [Fact]
//        public async Task Group_InvalidRoot_Throws()
//        {
//            TerminalRequest request = new(Guid.NewGuid().ToString(), "invalid_root1 grp1");
//            Func<Task> act = async () => await terminalRequestParser.ParseRequestAsync(request);
//            await act.Should().ThrowAsync<TerminalException>().WithMessage("The command owner is not valid. owner=invalid_root1 command=grp1.");
//        }

//        [Fact]
//        public async Task Group_NoRoot_Throws()
//        {
//            TerminalRequest request = new(Guid.NewGuid().ToString(), "grp1");
//            Func<Task> act = async () => await terminalRequestParser.ParseRequestAsync(request);
//            await act.Should().ThrowAsync<TerminalException>().WithMessage("The command owner is missing in the command request. owners=root1 command=grp1.");
//        }

//        [Fact]
//        public async Task Group_With_Root_With_Hierarchy_Parses()
//        {
//            terminalOptions.Parser.ParseHierarchy = true;

//            TerminalRequest request = new(Guid.NewGuid().ToString(), "root1 grp1");
//            var parsedCommand = await terminalRequestParser.ParseRequestAsync(request);

//            parsedCommand.Command.Id.Should().Be("grp1");
//            parsedCommand.Command.Name.Should().Be("grp1_name");
//            parsedCommand.Command.Description.Should().Be("grp1_desc");

//            parsedCommand.Hierarchy.Should().NotBeNull();
//            parsedCommand.Hierarchy!.IsDefault.Should().BeFalse();
//            parsedCommand.Hierarchy.LinkedCommand.Id.Should().Be("root1");
//            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

//            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
//            parsedCommand.Hierarchy.ChildGroup!.ChildGroup.Should().BeNull();
//            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().BeNull();
//            parsedCommand.Hierarchy.ChildGroup.LinkedCommand.Id.Should().Be("grp1");
//            parsedCommand.Hierarchy.ChildGroup.LinkedCommand.Should().BeSameAs(parsedCommand.Command);
//        }

//        [Fact]
//        public async Task Group_With_Root_Without_Hierarchy_Parses()
//        {
//            TerminalRequest request = new(Guid.NewGuid().ToString(), "root1 grp1");
//            var parsedCommand = await terminalRequestParser.ParseRequestAsync(request);

//            parsedCommand.Command.Id.Should().Be("grp1");
//            parsedCommand.Command.Name.Should().Be("grp1_name");
//            parsedCommand.Command.Description.Should().Be("grp1_desc");

//            parsedCommand.Hierarchy.Should().BeNull();
//        }

//        [Fact]
//        public async Task Invalid_Command_Throws()
//        {
//            Func<Task> func = async () => await terminalRequestParser.ParseRequestAsync(new TerminalRequest("id1", "root1 grp1 grp2 cmd_invalid"));
//            await func.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support any arguments. command=grp2");
//        }

//        [Fact]
//        public async Task Invalid_Group_Throws()
//        {
//            Func<Task> func = async () => await terminalRequestParser.ParseRequestAsync(new TerminalRequest("id1", "root1 grp1_invalid"));
//            await func.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support any arguments. command=root1");
//        }

//        [Fact]
//        public async Task Invalid_Nested_Group_Throws()
//        {
//            Func<Task> func = async () => await terminalRequestParser.ParseRequestAsync(new TerminalRequest("id1", "root1 grp1 grp2_invalid"));
//            await func.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support any arguments. command=grp1");
//        }

//        [Fact]
//        public async Task Invalid_Root_Throws()
//        {
//            Func<Task> func = async () => await terminalRequestParser.ParseRequestAsync(new TerminalRequest("id1", "root_invalid"));
//            await func.Should().ThrowAsync<TerminalException>().WithMessage($"The command is missing in the command request.");
//        }

//        [Fact]
//        public async Task Invalid_Root_With_Group_Throws()
//        {
//            Func<Task> func = async () => await terminalRequestParser.ParseRequestAsync(new TerminalRequest("id1", "root_invalid grp1"));
//            await func.Should().ThrowAsync<TerminalException>().WithMessage($"The command owner is not valid. owner=root_invalid command=grp1.");
//        }

//        [Fact]
//        public async Task Invalid_SubCommand_After_Valid_Command_Assumed_As_Argument()
//        {
//            TerminalRequest request = new(Guid.NewGuid().ToString(), $"root1 grp1 grp2 grp3 cmd2 invalid1");
//            Func<Task> act = async () => await terminalRequestParser.ParseRequestAsync(request);
//            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support any arguments. command=cmd2");
//        }

//        [Fact]
//        public async Task Invalid_SubCommand_Owner_Throws()
//        {
//            TerminalRequest request = new(Guid.NewGuid().ToString(), $"root1 grp1 grp2 grp3 invalid1 cmd2");
//            Func<Task> act = async () => await terminalRequestParser.ParseRequestAsync(request);
//            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The command owner is not valid. owner=invalid1 command=cmd2.");
//        }

//        [Fact]
//        public async Task Last_Invalid_Command_Is_Assumed_To_Be_Group_Argument()
//        {
//            TerminalRequest request = new(Guid.NewGuid().ToString(), $"root1 grp1 grp2 grp3 invalid_cmd1");
//            Func<Task> act = async () => await terminalRequestParser.ParseRequestAsync(request);
//            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support any arguments. command=grp3");
//        }

//        [Fact]
//        public async Task Last_Invalid_Group_Is_Assumed_To_Be_Root_Argument()
//        {
//            TerminalRequest request = new(Guid.NewGuid().ToString(), $"root1 invalid_grp1");
//            Func<Task> act = async () => await terminalRequestParser.ParseRequestAsync(request);
//            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support any arguments. command=root1");
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
