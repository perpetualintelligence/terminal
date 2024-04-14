/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using System;
using System.Linq;
using Xunit;

namespace OneImlx.Terminal.Hosting
{
    public class CommandBuilderTests
    {
        public CommandBuilderTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
        }

        [Fact]
        public void Build_Adds_Command_To_Global_ServiceCollection()
        {
            // Begin with no command
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalAsciiTextHandler());
            ServiceDescriptor? serviceDescriptor = terminalBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            serviceDescriptor.Should().BeNull();

            // Add command to local
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.SubCommand, CommandFlags.None).Checker<MockCommandChecker>();

            // Build
            ITerminalBuilder cliBuilderFromCommandBuilder = commandBuilder.Add();
            terminalBuilder.Should().BeSameAs(cliBuilderFromCommandBuilder);

            // Build adds to global
            serviceDescriptor = terminalBuilder.Services.First(e => e.ServiceType.Equals(typeof(CommandDescriptor)));
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
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalAsciiTextHandler());
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.SubCommand, CommandFlags.None).Checker<MockCommandChecker>();
            ITerminalBuilder cliBuilderFromCommandBuilder = commandBuilder.Add();
            terminalBuilder.Should().BeSameAs(cliBuilderFromCommandBuilder);
        }

        [Fact]
        public void NewBuilder_Returns_New_IServiceCollection()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalAsciiTextHandler());
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