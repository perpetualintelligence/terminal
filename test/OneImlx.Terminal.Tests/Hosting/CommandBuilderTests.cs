/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentAssertions;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Hosting
{
    public class CommandBuilderTests
    {
        public CommandBuilderTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder([]).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
        }

        [Fact]
        public void Add_Native_With_Owner_Throws()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "root1", "root1_desc", CommandType.NativeCommand, CommandFlags.None)
                                                            .Owners(new("owner1", "owner2"));

            Action act = () => commandBuilder.Add();
            act.Should().Throw<TerminalException>()
                        .WithErrorCode("invalid_command")
                        .WithErrorDescription("The command cannot have an owner. command_type=NativeCommand command=id1");
        }

        [Fact]
        public void Add_Root_With_Owner_Throws()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "root1", "root1_desc", CommandType.RootCommand, CommandFlags.None)
                                                            .Owners(new("owner1", "owner2"));

            Action act = () => commandBuilder.Add();
            act.Should().Throw<TerminalException>()
                        .WithErrorCode("invalid_command")
                        .WithErrorDescription("The command cannot have an owner. command_type=RootCommand command=id1");
        }

        [Fact]
        public void Build_Adds_Command_To_Global_ServiceCollection()
        {
            // Begin with no command
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ServiceDescriptor? serviceDescriptor = terminalBuilder.Services.FirstOrDefault(static e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            serviceDescriptor.Should().BeNull();

            // Add command to local
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.SubCommand, CommandFlags.None).Checker<MockCommandChecker>();

            // Build
            ITerminalBuilder cliBuilderFromCommandBuilder = commandBuilder.Add();
            terminalBuilder.Should().BeSameAs(cliBuilderFromCommandBuilder);

            // Build adds to global
            serviceDescriptor = terminalBuilder.Services.First(static e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();
            CommandDescriptor instance = (CommandDescriptor)serviceDescriptor.ImplementationInstance!;
            instance.Id.Should().Be("id1");
            instance.Name.Should().Be("name1");
            instance.Description.Should().Be("Command description");
        }

        [Fact]
        public void Build_Returns_Same_TerminalBuilder()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.SubCommand, CommandFlags.None).Checker<MockCommandChecker>();
            ITerminalBuilder cliBuilderFromCommandBuilder = commandBuilder.Add();
            terminalBuilder.Should().BeSameAs(cliBuilderFromCommandBuilder);
        }

        [Fact]
        public void NewBuilder_Returns_New_IServiceCollection()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII));
            CommandBuilder commandBuilder = new(terminalBuilder);
            commandBuilder.Services.Should().NotBeSameAs(serviceCollection);
        }

        ~CommandBuilderTests()
        {
            host.Dispose();
        }

        private void ConfigureServicesDelegate(IServiceCollection opt2)
        {
            serviceCollection = opt2;
        }

        private readonly IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}
