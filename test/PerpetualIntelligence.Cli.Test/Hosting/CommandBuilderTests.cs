/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Extensions;
using PerpetualIntelligence.Cli.Mocks;
using System;
using System.Linq;
using Xunit;

namespace PerpetualIntelligence.Cli.Hosting
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
            CliBuilder cliBuilder = new(serviceCollection);
            ServiceDescriptor? serviceDescriptor = cliBuilder.Services.FirstOrDefault(e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            serviceDescriptor.Should().BeNull();

            // Add command to local
            ICommandBuilder commandBuilder = cliBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "cmd name prefix", "Command description");

            // Build
            ICliBuilder cliBuilderFromCommandBuilder = commandBuilder.Add();
            cliBuilder.Should().BeSameAs(cliBuilderFromCommandBuilder);

            // Build adds to global
            serviceDescriptor = cliBuilder.Services.First(e => e.ServiceType.Equals(typeof(CommandDescriptor)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();
            CommandDescriptor instance = (CommandDescriptor)serviceDescriptor.ImplementationInstance!;
            instance.Id.Should().Be("id1");
            instance.Name.Should().Be("name1");
            instance.Prefix.Should().Be("cmd name prefix");
            instance.Description.Should().Be("Command description");
        }

        [Fact]
        public void Build_Returns_Same_CliBuilder()
        {
            CliBuilder cliBuilder = new(serviceCollection);
            ICommandBuilder commandBuilder = cliBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "cmd name prefix", "Command description");
            ICliBuilder cliBuilderFromCommandBuilder = commandBuilder.Add();
            cliBuilder.Should().BeSameAs(cliBuilderFromCommandBuilder);
        }

        [Fact]
        public void NewBuilder_Returns_New_IServiceCollection()
        {
            CliBuilder cliBuilder = new(serviceCollection);
            CommandBuilder commandBuilder = new(cliBuilder);
            commandBuilder.Services.Should().NotBeSameAs(serviceCollection);
        }

        ~CommandBuilderTests()
        {
            host.Dispose();
        }

        private void ConfigureServicesDelegate(IServiceCollection arg2)
        {
            serviceCollection = arg2;
        }

        private readonly IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}
