/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Cli.Extensions;
using PerpetualIntelligence.Cli.Mocks;
using System;
using Xunit;

namespace PerpetualIntelligence.Cli.Integration
{
    public class ArgumentBuilderTests : IDisposable
    {
        public ArgumentBuilderTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
        }

        [Fact]
        public void Build_Adds_Argument_To_CommandDescriptor()
        {
        }

        [Fact]
        public void Build_Returns_Same_CommandBuilder()
        {
            CliBuilder cliBuilder = new(serviceCollection);
            ICommandBuilder commandBuilder = cliBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "cmd name prefix", "Command description");

            ArgumentBuilder argumentBuilder = new (commandBuilder);
            ICommandBuilder cmdBuilderFromArgBuilder = argumentBuilder.Add();
            commandBuilder.Should().BeSameAs(cmdBuilderFromArgBuilder);
        }

        public void Dispose()
        {
            host.Dispose();
        }

        [Fact]
        public void NewBuilder_Returns_New_IServiceCollection()
        {
            CliBuilder cliBuilder = new(serviceCollection);
            CommandBuilder commandBuilder = new(cliBuilder);
            ArgumentBuilder argumentBuilder = new ArgumentBuilder(commandBuilder);

            commandBuilder.Services.Should().NotBeSameAs(serviceCollection);
            argumentBuilder.Services.Should().NotBeSameAs(serviceCollection);
            argumentBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);
        }

        private void ConfigureServicesDelegate(IServiceCollection arg2)
        {
            serviceCollection = arg2;
        }

        private IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}
