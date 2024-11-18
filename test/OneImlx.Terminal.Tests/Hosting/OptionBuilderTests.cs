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
    public class OptionBuilderTests : IDisposable
    {
        public OptionBuilderTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder([]).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
        }

        [Fact]
        public void Nos_OptionDescriptor_Throws()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalAsciiTextHandler());
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.SubCommand, CommandFlags.None).Checker<MockCommandChecker>();

            OptionBuilder optionBuilder = new(commandBuilder);
            Action act = () => optionBuilder.Add();
            act.Should().Throw<TerminalException>().WithMessage("The option builder is missing an option descriptor.");
        }

        [Fact]
        public void Build_Adds_OptionDescriptor_To_CommandDescriptor()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalAsciiTextHandler());
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.SubCommand, CommandFlags.None).Checker<MockCommandChecker>();

            commandBuilder.DefineOption("opt1", nameof(String), "test opt desc1", OptionFlags.None).Add()
                          .DefineOption("opt2", nameof(String), "test opt desc2", OptionFlags.None).Add()
                          .DefineOption("opt3", nameof(String), "test opt desc3", OptionFlags.None).Add();

            ServiceProvider serviceProvider = commandBuilder.Services.BuildServiceProvider();
            var optDescriptors = serviceProvider.GetServices<OptionDescriptor>();
            optDescriptors.Count().Should().Be(3);
            optDescriptors.Should().Contain(static x => x.Id == "opt1");
            optDescriptors.Should().Contain(static x => x.Id == "opt2");
            optDescriptors.Should().Contain(static x => x.Id == "opt3");
        }

        [Fact]
        public void Build_Returns_Same_CommandBuilder()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalAsciiTextHandler());
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.SubCommand, CommandFlags.None).Checker<MockCommandChecker>();

            IOptionBuilder optionBuilder = commandBuilder.DefineOption("opt1", nameof(String), "test opt desc1", OptionFlags.None);
            ICommandBuilder cmdBuilderFromArgBuilder = optionBuilder.Add();
            commandBuilder.Should().BeSameAs(cmdBuilderFromArgBuilder);
        }

        public void Dispose()
        {
            host.Dispose();
        }

        [Fact]
        public void NewBuilder_Returns_New_IServiceCollection()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalAsciiTextHandler());
            CommandBuilder commandBuilder = new(terminalBuilder);
            OptionBuilder argumentBuilder = new(commandBuilder);

            commandBuilder.Services.Should().NotBeSameAs(serviceCollection);
            argumentBuilder.Services.Should().NotBeSameAs(serviceCollection);
            argumentBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);
        }

        private void ConfigureServicesDelegate(IServiceCollection opt2)
        {
            serviceCollection = opt2;
        }

        private readonly IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}