/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/
/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Extensions;
using PerpetualIntelligence.Terminal.Mocks;
using System;
using Xunit;

namespace PerpetualIntelligence.Terminal.Hosting
{
    public class OptionBuilderTests : IDisposable
    {
        public OptionBuilderTests()
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
            TerminalBuilder terminalBuilder = new(serviceCollection);
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "Command description", CommandType.SubCommand, CommandFlags.None);

            OptionBuilder argumentBuilder = new(commandBuilder);
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
            TerminalBuilder terminalBuilder = new(serviceCollection);
            CommandBuilder commandBuilder = new(terminalBuilder);
            OptionBuilder argumentBuilder = new(commandBuilder);

            commandBuilder.Services.Should().NotBeSameAs(serviceCollection);
            argumentBuilder.Services.Should().NotBeSameAs(serviceCollection);
            argumentBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);
        }

        private void ConfigureServicesDelegate(IServiceCollection arg2)
        {
            serviceCollection = arg2;
        }

        private readonly IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}