/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using System;
using System.Linq;
using Xunit;

namespace OneImlx.Terminal.Extensions
{
    public class ICommandBuilderExtensionsTests
    {
        public ICommandBuilderExtensionsTests()
        {
            IServiceCollection? serviceDescriptors = null;

            using var host = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(arg =>
            {
                serviceDescriptors = arg;
            }).Build();

            serviceDescriptors.Should().NotBeNull();
            terminalBuilder = serviceDescriptors!.CreateTerminalBuilder(new TerminalAsciiTextHandler());
            commandBuilder = terminalBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "description1", CommandType.SubCommand, CommandFlags.None);
        }

        [Fact]
        public void AddArgument_Adds_Custom_DataType_Correctly()
        {
            IOptionBuilder argumentBuilder = commandBuilder.DefineOption("opt1", "custom-dt", "description1", OptionFlags.Disabled, alias: null);

            // Option builder, command builder have different service collections.
            argumentBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);

            ServiceDescriptor serviceDescriptor = argumentBuilder.Services.First(e => e.ServiceType.Equals(typeof(OptionDescriptor)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();

            OptionDescriptor option = (OptionDescriptor)serviceDescriptor.ImplementationInstance!;
            option.Id.Should().Be("opt1");
            option.DataType.Should().Be("custom-dt");
            option.Description.Should().Be("description1");
            option.Alias.Should().BeNull();
            option.Flags.Should().Be(OptionFlags.Disabled);
        }

        [Fact]
        public void AddArgument_Adds_Std_DataType_Correctly()
        {
            IOptionBuilder argumentBuilder = commandBuilder.DefineOption("opt1", nameof(Int32), "description1", OptionFlags.Required | OptionFlags.Obsolete, alias: "arg-alias1");

            // Option builder, command builder have different service collections.
            argumentBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);

            ServiceDescriptor serviceDescriptor = argumentBuilder.Services.First(e => e.ServiceType.Equals(typeof(OptionDescriptor)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();

            OptionDescriptor option = (OptionDescriptor)serviceDescriptor.ImplementationInstance!;
            option.Id.Should().Be("opt1");
            option.DataType.Should().Be(nameof(Int32));
            option.Description.Should().Be("description1");
            option.Alias.Should().Be("arg-alias1");
            option.Flags.Should().Be(OptionFlags.Required | OptionFlags.Obsolete);
        }

        private ITerminalBuilder terminalBuilder = null!;
        private ICommandBuilder commandBuilder = null!;
    }
}