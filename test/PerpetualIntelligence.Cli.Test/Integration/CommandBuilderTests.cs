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
    public class CommandBuilderTests
    {
        public CommandBuilderTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
        }

        [Fact]
        public void CommandBuilder_Build_ShouldAdd_ToGlobalServiceCollection()
        {
        }

        [Fact]
        public void Build_Returns_Same_CliBuilder()
        {
            CliBuilder cliBuilder = new(serviceCollection);
            ICommandBuilder commandBuilder = cliBuilder.AddCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "cmd name prefix", "Command description");
            ICliBuilder cliBuilderFromCommandBuilder = commandBuilder.Build();
            cliBuilder.Should().BeSameAs(cliBuilderFromCommandBuilder);
        }

        [Fact]
        public void NewCommand_Returns_New_IServiceCollection()
        {
            CliBuilder cliBuilder = new(serviceCollection);
            CommandBuilder commandBuilder = new (cliBuilder);
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

        private IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}
