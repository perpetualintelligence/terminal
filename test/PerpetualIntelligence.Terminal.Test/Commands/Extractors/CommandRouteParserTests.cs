/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Stores;
using PerpetualIntelligence.Terminal.Stores.InMemory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    public class CommandRouteParserTests
    {
        public CommandRouteParserTests()
        {
            terminalOptions = MockTerminalOptions.NewAliasOptions();
            textHandler = new AsciiTextHandler();
            commandDescriptors = new Dictionary<string, CommandDescriptor>()
            {
               { "root1", new CommandDescriptor("root1", "root1_name", "root1_desc", CommandType.Root, CommandFlags.None) },
               { "grp1", new CommandDescriptor("grp1", "grp1_name", "grp1_desc", CommandType.Group, CommandFlags.None) },
               { "grp2", new CommandDescriptor("grp2", "grp2_name", "grp2_desc", CommandType.Group, CommandFlags.None) },
               { "grp3", new CommandDescriptor("grp3", "grp3_name", "grp3_desc", CommandType.Group, CommandFlags.None) },
               { "cmd1", new CommandDescriptor("cmd1", "cmd1_name", "cmd1_desc", CommandType.SubCommand, CommandFlags.None) },
               { "cmd2", new CommandDescriptor("cmd2", "cmd2_name", "cmd2_desc", CommandType.SubCommand, CommandFlags.None) },
               { "cmd_nr1", new CommandDescriptor("cmd_nr1", "cmd_nr1_name", "cmd_nr1_desc", CommandType.SubCommand, CommandFlags.None) },
               { "cmd_nr2", new CommandDescriptor("cmd_nr2", "cmd_nr2_name", "cmd_nr2_desc", CommandType.SubCommand, CommandFlags.None) }
            };
            commandStoreHandler = new InMemoryCommandStore(commandDescriptors);
            logger = new NullLogger<CommandRouteParser>();

            commandRouteParser = new CommandRouteParser(textHandler, commandStoreHandler, terminalOptions, logger);
        }

        [Theory]
        [InlineData("cmd_nr1")]
        [InlineData("cmd_nr2")]
        public async Task Command_NoGroup_NoRoot_Extracts_With_Default_Root(string cmdRoute)
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), cmdRoute);
            var parsedCommand = await commandRouteParser.ParseAsync(commandRoute);

            parsedCommand.Command.Id.Should().Be(cmdRoute);
            parsedCommand.Command.Name.Should().Be($"{cmdRoute}_name");
            parsedCommand.Command.Description.Should().Be($"{cmdRoute}_desc");

            parsedCommand.Hierarchy.IsDefault.Should().BeTrue();
            parsedCommand.Hierarchy.LinkedCommand.Id.Should().Be("default");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();
            parsedCommand.Hierarchy.ChildGroup.Should().BeNull();
        }

        [Fact]
        public async Task Group_NoRoot_Throws()
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), "grp1");
            Func<Task> act = async () => await commandRouteParser.ParseAsync(commandRoute);
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The command group must be preceded by a root command. group=grp1");
        }

        [Fact]
        public async Task Group_With_Root_Parses()
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), "root1 grp1");
            var parsedCommand = await commandRouteParser.ParseAsync(commandRoute);

            parsedCommand.Command.Id.Should().Be("grp1");
            parsedCommand.Command.Name.Should().Be("grp1_name");
            parsedCommand.Command.Description.Should().Be("grp1_desc");

            parsedCommand.Hierarchy.IsDefault.Should().BeFalse();
            parsedCommand.Hierarchy.LinkedCommand.Id.Should().Be("root1");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.ChildGroup.Should().BeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().BeNull();
            parsedCommand.Hierarchy.ChildGroup.LinkedCommand.Id.Should().Be("grp1");
            parsedCommand.Hierarchy.ChildGroup.LinkedCommand.Should().BeSameAs(parsedCommand.Command);
        }

        [Fact]
        public async Task Command_With_Group_With_Root_Parses()
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), "root1 grp1 cmd1");
            var parsedCommand = await commandRouteParser.ParseAsync(commandRoute);

            parsedCommand.Command.Id.Should().Be("cmd1");
            parsedCommand.Command.Name.Should().Be("cmd1_name");
            parsedCommand.Command.Description.Should().Be("cmd1_desc");

            parsedCommand.Hierarchy.IsDefault.Should().BeFalse();
            parsedCommand.Hierarchy.LinkedCommand.Id.Should().Be("root1");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.ChildGroup.Should().BeNull();
            parsedCommand.Hierarchy.ChildGroup.LinkedCommand.Id.Should().Be("grp1");

            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand!.LinkedCommand.Should().BeSameAs(parsedCommand.Command);
        }

        [Fact]
        public async Task Command_With_Nested_Groups_With_Root_Parses()
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), "root1 grp1 grp2 grp3 cmd1");
            var parsedCommand = await commandRouteParser.ParseAsync(commandRoute);

            parsedCommand.Command.Id.Should().Be("cmd1");
            parsedCommand.Command.Name.Should().Be("cmd1_name");
            parsedCommand.Command.Description.Should().Be("cmd1_desc");

            parsedCommand.Hierarchy.IsDefault.Should().BeFalse();
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

        [Theory]
        [InlineData("root1 grp1 grp2 grp3 cmd1 cmd2")]
        [InlineData("root1 grp1 cmd1 cmd2")]
        [InlineData("root1 cmd1 cmd2")]
        [InlineData("cmd1 cmd2")]
        public async Task Nested_SubCommands_Throws(string cmdString)
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), cmdString);
            Func<Task> act = async () => await commandRouteParser.ParseAsync(commandRoute);
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The nested subcommands are not supported. command=cmd2");
        }

        [Theory]
        [InlineData("root1 grp1 cmd1 grp2 grp3")]
        [InlineData("cmd1 root1 grp1")]
        [InlineData("root1 cmd1 grp1")]
        [InlineData("grp1 root1")]
        public async Task Invalid_Command_Order_Throws(string cmdString)
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), cmdString);
            Func<Task> act = async () => await commandRouteParser.ParseAsync(commandRoute);
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The nested subcommands are not supported. command=cmd2");
        }


        [Theory]
        [InlineData("root1 grp1 grp1 grp3 cmd1 cmd2")]
        [InlineData("root1 grp1 cmd1 cmd2")]
        [InlineData("root1 cmd1 cmd2")]
        [InlineData("cmd1 cmd2")]
        [InlineData("root1 root1")]
        [InlineData("root1 grp1 grp2 grp2")]
        public async Task Duplicate_Commands_Throws(string cmdString)
        {
            CommandRoute commandRoute = new(Guid.NewGuid().ToString(), cmdString);
            Func<Task> act = async () => await commandRouteParser.ParseAsync(commandRoute);
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The nested subcommands are not supported. command=cmd2");
        }

        private TerminalOptions terminalOptions;
        private ITextHandler textHandler;
        private ICommandStoreHandler commandStoreHandler;
        private Dictionary<string, CommandDescriptor> commandDescriptors;
        private ICommandRouteParser commandRouteParser;
        private ILogger<CommandRouteParser> logger;
    }
}