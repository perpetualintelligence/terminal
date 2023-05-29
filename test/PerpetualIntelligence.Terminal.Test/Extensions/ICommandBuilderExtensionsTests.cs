/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Hosting;
using PerpetualIntelligence.Terminal.Mocks;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace PerpetualIntelligence.Terminal.Extensions
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
            cliBuilder = serviceDescriptors!.AddCliBuilder();
            commandBuilder = cliBuilder.DefineCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "description1");
        }

        [Fact]
        public void AddArgument_Adds_Custom_DataType_Correctly()
        {
            IOptionBuilder argumentBuilder = commandBuilder.DefineOption("arg1", "custom-dt", "description1", alias: null, defaultValue: null, required: false, disabled: true, obsolete: false);

            // Option builder, command builder have different service collections.
            argumentBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);

            ServiceDescriptor serviceDescriptor = argumentBuilder.Services.First(e => e.ServiceType.Equals(typeof(OptionDescriptor)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();

            OptionDescriptor option = (OptionDescriptor)serviceDescriptor.ImplementationInstance!;
            option.Id.Should().Be("arg1");
            option.DataType.Should().Be(DataType.Custom);
            option.CustomDataType.Should().Be("custom-dt");
            option.Description.Should().Be("description1");
            option.Alias.Should().BeNull();
            option.DefaultValue.Should().BeNull();
            option.Required.Should().BeFalse();
            option.Disabled.Should().BeTrue();
            option.Obsolete.Should().BeFalse();
        }

        [Fact]
        public void AddArgument_Adds_Std_DataType_Correctly()
        {
            IOptionBuilder argumentBuilder = commandBuilder.DefineOption("arg1", DataType.CreditCard, "description1", alias: "arg-alias1", defaultValue: 4444555544445555, required: true, disabled: false, obsolete: true);

            // Option builder, command builder have different service collections.
            argumentBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);

            ServiceDescriptor serviceDescriptor = argumentBuilder.Services.First(e => e.ServiceType.Equals(typeof(OptionDescriptor)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();

            OptionDescriptor option = (OptionDescriptor)serviceDescriptor.ImplementationInstance!;
            option.Id.Should().Be("arg1");
            option.DataType.Should().Be(DataType.CreditCard);
            option.CustomDataType.Should().BeNull();
            option.Description.Should().Be("description1");
            option.Alias.Should().Be("arg-alias1");
            option.DefaultValue.Should().Be(4444555544445555);
            option.Required.Should().BeTrue();
            option.Disabled.Should().BeFalse();
            option.Obsolete.Should().BeTrue();
        }

        private ITerminalBuilder cliBuilder = null!;
        private ICommandBuilder commandBuilder = null!;
    }
}
