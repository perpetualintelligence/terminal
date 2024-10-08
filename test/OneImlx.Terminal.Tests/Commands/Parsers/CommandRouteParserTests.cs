﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Parsers
{
    public class CommandRouteParserTests
    {
        public CommandRouteParserTests()
        {
            terminalOptions = MockTerminalOptions.NewAliasOptions();
            textHandler = new TerminalAsciiTextHandler();
            commandDescriptors = new(textHandler, new List<CommandDescriptor>()
            {
               new CommandDescriptor("root1", "root1_name", "root1_desc", CommandType.Root, CommandFlags.None),
               new CommandDescriptor("grp1", "grp1_name", "grp1_desc", CommandType.Group, CommandFlags.None, new OwnerIdCollection("root1")),
               new CommandDescriptor("grp2", "grp2_name", "grp2_desc", CommandType.Group, CommandFlags.None, new OwnerIdCollection("grp1")),
               new CommandDescriptor("grp3", "grp3_name", "grp3_desc", CommandType.Group, CommandFlags.None, new OwnerIdCollection("grp2")),
               new CommandDescriptor("cmd1", "cmd1_name", "cmd1_desc", CommandType.SubCommand, CommandFlags.None, new OwnerIdCollection("grp3")),
               new CommandDescriptor("cmd2", "cmd2_name", "cmd2_desc", CommandType.SubCommand, CommandFlags.None, new OwnerIdCollection("grp3")),
               new CommandDescriptor("cmd_nr1", "cmd_nr1_name", "cmd_nr1_desc", CommandType.SubCommand, CommandFlags.None),
               new CommandDescriptor("cmd_nr2", "cmd_nr2_name", "cmd_nr2_desc", CommandType.SubCommand, CommandFlags.None)
            });
            commandStore = new TerminalInMemoryCommandStore(textHandler, commandDescriptors.Values);
            logger = new NullLogger<CommandRouteParser>();

            commandRouteParser = new CommandRouteParser(textHandler, commandStore, terminalOptions, logger);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(null)]
        public async Task Does_Not_Parse_Hierarchy_If_Option_Not_Set(bool? parseHierarchy)
        {
            terminalOptions.Parser.ParseHierarchy = parseHierarchy;

            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), "root1");
            var parsedCommand = await commandRouteParser.ParseRouteAsync(commandRoute);
            parsedCommand.Hierarchy.Should().BeNull();

            commandRoute = new(Guid.NewGuid().ToString(), "root1 grp1");
            parsedCommand = await commandRouteParser.ParseRouteAsync(commandRoute);
            parsedCommand.Hierarchy.Should().BeNull();

            commandRoute = new(Guid.NewGuid().ToString(), "root1 grp1 grp2");
            parsedCommand = await commandRouteParser.ParseRouteAsync(commandRoute);
            parsedCommand.Hierarchy.Should().BeNull();

            commandRoute = new(Guid.NewGuid().ToString(), "root1 grp1 grp2 grp3");
            parsedCommand = await commandRouteParser.ParseRouteAsync(commandRoute);
            parsedCommand.Hierarchy.Should().BeNull();

            commandRoute = new(Guid.NewGuid().ToString(), "root1 grp1 grp2 grp3 cmd1");
            parsedCommand = await commandRouteParser.ParseRouteAsync(commandRoute);
            parsedCommand.Hierarchy.Should().BeNull();
        }

        [Theory]
        [InlineData("cmd_nr1")]
        [InlineData("cmd_nr2")]
        public async Task Command_NoHierarchy_NoGroup_NoRoot_Extracts_Without_Default_Root(string cmdRoute)
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), cmdRoute);
            var parsedCommand = await commandRouteParser.ParseRouteAsync(commandRoute);

            parsedCommand.Command.Id.Should().Be(cmdRoute);
            parsedCommand.Command.Name.Should().Be($"{cmdRoute}_name");
            parsedCommand.Command.Description.Should().Be($"{cmdRoute}_desc");

            parsedCommand.Hierarchy.Should().BeNull();
        }

        [Theory]
        [InlineData("cmd_nr1")]
        [InlineData("cmd_nr2")]
        public async Task Command_WithHierarchy_NoGroup_NoRoot_Extracts_With_Default_Root(string cmdRoute)
        {
            terminalOptions.Parser.ParseHierarchy = true;

            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), cmdRoute);
            var parsedCommand = await commandRouteParser.ParseRouteAsync(commandRoute);

            parsedCommand.Command.Id.Should().Be(cmdRoute);
            parsedCommand.Command.Name.Should().Be($"{cmdRoute}_name");
            parsedCommand.Command.Description.Should().Be($"{cmdRoute}_desc");

            parsedCommand.Hierarchy.Should().NotBeNull();
            parsedCommand.Hierarchy!.IsDefault.Should().BeTrue();
            parsedCommand.Hierarchy.ChildGroup.Should().BeNull();
            parsedCommand.Hierarchy.ChildSubCommand.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildSubCommand!.LinkedCommand.Should().BeSameAs(parsedCommand.Command);
            parsedCommand.Hierarchy.LinkedCommand.Id.Should().Be("default");
        }

        [Fact]
        public async Task CommandRoute_Is_Set_In_Result()
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), "root1");
            var result = await commandRouteParser.ParseRouteAsync(commandRoute);
            result.Should().NotBeNull();

            result.CommandRoute.Should().NotBeNull();
            result.CommandRoute.Should().BeSameAs(commandRoute);
        }

        [Fact]
        public async Task Root_No_Hierarchy_Parses_Correctly()
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), "root1");
            var result = await commandRouteParser.ParseRouteAsync(commandRoute);
            result.Should().NotBeNull();
            result.Hierarchy.Should().BeNull();

            result.Command.Id.Should().Be("root1");
        }

        [Fact]
        public async Task Root_With_Hierarchy_Parses_Correctly()
        {
            terminalOptions.Parser.ParseHierarchy = true;

            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), "root1");
            var result = await commandRouteParser.ParseRouteAsync(commandRoute);
            result.Should().NotBeNull();

            result.Command.Id.Should().Be("root1");

            result.Hierarchy.Should().NotBeNull();
            result.Hierarchy!.ChildSubCommand.Should().BeNull();
            result.Hierarchy.ChildGroup.Should().BeNull();
            result.Hierarchy.LinkedCommand.Should().BeSameAs(result.Command);
        }

        [Fact]
        public async Task Group_NoRoot_Throws()
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), "grp1");
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(commandRoute);
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The command owner is missing in the command route. owners=root1 command=grp1.");
        }

        [Fact]
        public async Task Group_InvalidRoot_Throws()
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), "invalid_root1 grp1");
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(commandRoute);
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The command owner is not valid. owner=invalid_root1 command=grp1.");
        }

        [Theory]
        [InlineData("cmd1", "cmd2")]
        [InlineData("cmd2", "cmd1")]
        [InlineData("cmd1", "cmd1")]
        [InlineData("cmd2", "cmd2")]
        public async Task Nested_SubCommand_Throws(string cmd1, string cmd2)
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), $"root1 grp1 grp2 grp3 {cmd1} {cmd2}");
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(commandRoute);
            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The command owner is not valid. owner={cmd1} command={cmd2}.");
        }

        [Fact]
        public async Task Invalid_SubCommand_Owner_Throws()
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), $"root1 grp1 grp2 grp3 invalid1 cmd2");
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(commandRoute);
            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The command owner is not valid. owner=invalid1 command=cmd2.");
        }

        [Fact]
        public async Task Invalid_SubCommand_After_Valid_Command_Assumed_As_Argument()
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), $"root1 grp1 grp2 grp3 cmd2 invalid1");
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(commandRoute);
            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support any arguments. command=cmd2");
        }

        [Fact]
        public async Task Last_Invalid_Command_Is_Assumed_To_Be_Group_Argument()
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), $"root1 grp1 grp2 grp3 invalid_cmd1");
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(commandRoute);
            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support any arguments. command=grp3");
        }

        [Fact]
        public async Task Last_Invalid_Group_Is_Assumed_To_Be_Root_Argument()
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), $"root1 invalid_grp1");
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(commandRoute);
            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support any arguments. command=root1");
        }

        [Theory]
        [InlineData("root1    grp1   grp2  grp3      cmd1")]
        [InlineData("   root1    grp1   grp2  grp3      cmd1")]
        [InlineData("   root1    grp1   grp2  grp3      cmd1    ")]
        [InlineData("root1    grp1   grp2  grp3      cmd1    ")]
        public async Task Multiple_Separators_Between_Commands_Ignored(string raw)
        {
            terminalOptions.Parser.ParseHierarchy = true;

            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), raw);
            var parsedCommand = await commandRouteParser.ParseRouteAsync(commandRoute);

            parsedCommand.Command.Id.Should().Be("cmd1");

            parsedCommand.Hierarchy.Should().NotBeNull();
        }

        [Fact]
        public async Task Group_InvalidNestedRoot_Throws()
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), "root1 grp1 invalid_grp2 grp3");
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(commandRoute);
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The command owner is not valid. owner=invalid_grp2 command=grp3.");
        }

        [Fact]
        public async Task Group_With_Root_Without_Hierarchy_Parses()
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), "root1 grp1");
            var parsedCommand = await commandRouteParser.ParseRouteAsync(commandRoute);

            parsedCommand.Command.Id.Should().Be("grp1");
            parsedCommand.Command.Name.Should().Be("grp1_name");
            parsedCommand.Command.Description.Should().Be("grp1_desc");

            parsedCommand.Hierarchy.Should().BeNull();
        }

        [Fact]
        public async Task Group_With_Root_With_Hierarchy_Parses()
        {
            terminalOptions.Parser.ParseHierarchy = true;

            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), "root1 grp1");
            var parsedCommand = await commandRouteParser.ParseRouteAsync(commandRoute);

            parsedCommand.Command.Id.Should().Be("grp1");
            parsedCommand.Command.Name.Should().Be("grp1_name");
            parsedCommand.Command.Description.Should().Be("grp1_desc");

            parsedCommand.Hierarchy.Should().NotBeNull();
            parsedCommand.Hierarchy!.IsDefault.Should().BeFalse();
            parsedCommand.Hierarchy.LinkedCommand.Id.Should().Be("root1");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.ChildGroup.Should().BeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().BeNull();
            parsedCommand.Hierarchy.ChildGroup.LinkedCommand.Id.Should().Be("grp1");
            parsedCommand.Hierarchy.ChildGroup.LinkedCommand.Should().BeSameAs(parsedCommand.Command);
        }

        [Fact]
        public async Task Command_With_Nested_Groups_And_Root_With_Hierarchy_Parses()
        {
            terminalOptions.Parser.ParseHierarchy = true;

            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), "root1 grp1 grp2 grp3 cmd1");
            var parsedCommand = await commandRouteParser.ParseRouteAsync(commandRoute);

            parsedCommand.Command.Id.Should().Be("cmd1");
            parsedCommand.Command.Name.Should().Be("cmd1_name");
            parsedCommand.Command.Description.Should().Be("cmd1_desc");

            parsedCommand.Hierarchy.Should().NotBeNull();
            parsedCommand.Hierarchy!.IsDefault.Should().BeFalse();
            parsedCommand.Hierarchy.LinkedCommand.Id.Should().Be("root1");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.LinkedCommand.Id.Should().Be("grp1");
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildGroup!.LinkedCommand.Id.Should().Be("grp2");
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.ChildGroup.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.ChildGroup!.LinkedCommand.Id.Should().Be("grp3");

            parsedCommand.Hierarchy.ChildGroup.ChildGroup.ChildGroup.ChildSubCommand.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.ChildGroup.ChildSubCommand!.LinkedCommand.Should().BeSameAs(parsedCommand.Command);
        }

        [Fact]
        public async Task Command_With_Nested_Groups_And_Root_Without_Hierarchy_Parses()
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), "root1 grp1 grp2 grp3 cmd1");
            var parsedCommand = await commandRouteParser.ParseRouteAsync(commandRoute);

            parsedCommand.Command.Id.Should().Be("cmd1");
            parsedCommand.Command.Name.Should().Be("cmd1_name");
            parsedCommand.Command.Description.Should().Be("cmd1_desc");

            parsedCommand.Hierarchy.Should().BeNull();
        }

        [Theory]
        [InlineData("root1 grp1 grp2 grp3 cmd1 cmd2", "cmd1", "cmd2")]
        [InlineData("cmd_nr1 cmd_nr2", "cmd_nr1", "cmd_nr2")]
        public async Task Nested_SubCommands_Throws(string cmdString, string errOwner, string errCmd)
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), cmdString);
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(commandRoute);
            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The command owner is not valid. owner={errOwner} command={errCmd}.");
        }

        [Theory]
        [InlineData("root1 grp1 grp2 grp3 cmd1 cmd1", "cmd1")]
        [InlineData("root1 grp1 grp2 grp3 grp3", "grp3")]
        [InlineData("root1 grp1 grp2 grp2", "grp2")]
        [InlineData("root1 grp1 grp1", "grp1")]
        [InlineData("root1 root1", "root1")]
        public async Task Duplicate_Commands_Throws(string cmdString, string duplicateCmd)
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), cmdString);
            Func<Task> act = async () => await commandRouteParser.ParseRouteAsync(commandRoute);
            await act.Should().ThrowAsync<TerminalException>().WithMessage($"The command owner is not valid. owner={duplicateCmd} command={duplicateCmd}.");
        }

        [Theory]
        [InlineData("@root1 grp1 grp2 grp3 cmd1 cmd2", "@root1", "grp1")]
        [InlineData("root1 #grp1 grp2 grp3 cmd1 cmd2", "#grp1", "grp2")]
        [InlineData("root1 grp1 $grp2 grp3 cmd1 cmd2", "$grp2", "grp3")]
        [InlineData("root1 grp1 grp2 grp3! cmd1 cmd2", "grp3!", "cmd1")]
        public async Task Unexpected_Inputs_Should_Throw(string cmdStr, string errCmd, string childCmd)
        {
            Func<Task> func = async () => await commandRouteParser.ParseRouteAsync(new CommandRoute("id1", cmdStr));
            await func.Should().ThrowAsync<TerminalException>().WithMessage($"The command owner is not valid. owner={errCmd} command={childCmd}.");
        }

        [Theory]
        [InlineData("root1", "root1")]
        [InlineData("root1 grp1", "grp1")]
        [InlineData("root1 grp1 grp2", "grp2")]
        [InlineData("root1 grp1 grp2 grp3", "grp3")]
        [InlineData("root1 grp1 grp2 grp3 cmd1", "cmd1")]
        public async Task Arguments_NotSupported_Throws(string cmdStr, string errCmd)
        {
            Func<Task> func = async () => await commandRouteParser.ParseRouteAsync(new CommandRoute("id1", $"{cmdStr} \"not supported arg1\" 36.25"));
            await func.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support any arguments. command={errCmd}");
        }

        [Theory]
        [InlineData("root1", "root1")]
        [InlineData("root1 grp1", "grp1")]
        [InlineData("root1 grp1 grp2", "grp2")]
        [InlineData("root1 grp1 grp2 grp3", "grp3")]
        [InlineData("root1 grp1 grp2 grp3 cmd1", "cmd1")]
        public async Task Options_NotSupported_Throws(string cmdStr, string errCmd)
        {
            Func<Task> func = async () => await commandRouteParser.ParseRouteAsync(new CommandRoute("id1", $"{cmdStr} --opt1 val1 -opt2 val2"));
            await func.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support any options. command={errCmd}");
        }

        [Fact]
        public async Task Invalid_Root_Throws()
        {
            Func<Task> func = async () => await commandRouteParser.ParseRouteAsync(new CommandRoute("id1", "root_invalid"));
            await func.Should().ThrowAsync<TerminalException>().WithMessage($"The command is missing in the command route.");
        }

        [Fact]
        public async Task Invalid_Root_With_Group_Throws()
        {
            Func<Task> func = async () => await commandRouteParser.ParseRouteAsync(new CommandRoute("id1", "root_invalid grp1"));
            await func.Should().ThrowAsync<TerminalException>().WithMessage($"The command owner is not valid. owner=root_invalid command=grp1.");
        }

        [Fact]
        public async Task Invalid_Group_Throws()
        {
            Func<Task> func = async () => await commandRouteParser.ParseRouteAsync(new CommandRoute("id1", "root1 grp1_invalid"));
            await func.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support any arguments. command=root1");
        }

        [Fact]
        public async Task Invalid_Nested_Group_Throws()
        {
            Func<Task> func = async () => await commandRouteParser.ParseRouteAsync(new CommandRoute("id1", "root1 grp1 grp2_invalid"));
            await func.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support any arguments. command=grp1");
        }

        [Fact]
        public async Task Invalid_Command_Throws()
        {
            Func<Task> func = async () => await commandRouteParser.ParseRouteAsync(new CommandRoute("id1", "root1 grp1 grp2 cmd_invalid"));
            await func.Should().ThrowAsync<TerminalException>().WithMessage($"The command does not support any arguments. command=grp2");
        }

        private readonly TerminalOptions terminalOptions;
        private readonly ITerminalTextHandler textHandler;
        private readonly ITerminalCommandStore commandStore;
        private readonly CommandDescriptors commandDescriptors;
        private readonly ICommandRouteParser commandRouteParser;
        private readonly ILogger<CommandRouteParser> logger;
    }
}