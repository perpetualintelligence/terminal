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
    public class ArgumentBuilderTests : IDisposable
    {
        public ArgumentBuilderTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder([]).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
        }

        [Fact]
        public void No_Argument_Descriptor_Throws()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalAsciiTextHandler());
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.SubCommand, CommandFlags.None).Checker<MockCommandChecker>();

            ArgumentBuilder argumentBuilder = new(commandBuilder);
            Action act = () => argumentBuilder.Add();
            act.Should().Throw<TerminalException>().WithMessage("The argument builder is missing an argument descriptor.");
        }

        [Fact]
        public void Build_Adds_ArgumentDescriptor_To_CommandDescriptor()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalAsciiTextHandler());
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.SubCommand, CommandFlags.None).Checker<MockCommandChecker>();

            commandBuilder.DefineArgument(1, "arg1", nameof(String), "test arg desc1", ArgumentFlags.None).Add()
                          .DefineArgument(2, "arg2", nameof(String), "test arg desc2", ArgumentFlags.None).Add()
                          .DefineArgument(3, "arg3", nameof(String), "test arg desc3", ArgumentFlags.None).Add();

            ServiceProvider serviceProvider = commandBuilder.Services.BuildServiceProvider();
            var argDescriptors = serviceProvider.GetServices<ArgumentDescriptor>();
            argDescriptors.Count().Should().Be(3);
            argDescriptors.Should().Contain(x => x.Id == "arg1");
            argDescriptors.Should().Contain(x => x.Id == "arg2");
            argDescriptors.Should().Contain(x => x.Id == "arg3");
        }

        [Fact]
        public void Build_Returns_Same_CommandBuilder()
        {
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalAsciiTextHandler());
            ICommandBuilder commandBuilder = terminalBuilder.DefineCommand<MockCommandRunner>("id1", "name1", "Command description", CommandType.SubCommand, CommandFlags.None).Checker<MockCommandChecker>();

            IArgumentBuilder argumentBuilder = commandBuilder.DefineArgument(1, "arg1", nameof(String), "test arg desc1", ArgumentFlags.None);
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
            TerminalBuilder terminalBuilder = new(serviceCollection, new TerminalAsciiTextHandler());
            CommandBuilder commandBuilder = new(terminalBuilder);
            ArgumentBuilder argumentBuilder = new(commandBuilder);

            commandBuilder.Services.Should().NotBeSameAs(serviceCollection);
            argumentBuilder.Services.Should().NotBeSameAs(serviceCollection);
            argumentBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);
        }

        private void ConfigureServicesDelegate(IServiceCollection services)
        {
            serviceCollection = services;
        }

        private readonly IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}