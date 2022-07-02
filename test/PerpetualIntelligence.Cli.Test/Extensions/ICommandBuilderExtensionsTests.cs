/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Integration;
using PerpetualIntelligence.Cli.Mocks;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace PerpetualIntelligence.Cli.Extensions
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
            commandBuilder = cliBuilder.AddCommand<MockCommandChecker, MockCommandRunner>("id1", "name1", "prefix1", "description1");
        }

        [Fact]
        public void AddArgument_Adds_Std_DataType_Correctly()
        {
            IArgumentBuilder argumentBuilder = commandBuilder.AddArgument("arg1", DataType.CreditCard, "description1", alias: "arg-alias1", defaultValue: 4444555544445555, required: true, disabled: false, obsolete: true);

            // Argument builder, command builder have different service collections.
            argumentBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);

            ServiceDescriptor serviceDescriptor = argumentBuilder.Services.First(e => e.ServiceType.Equals(typeof(ArgumentDescriptor)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();

            ArgumentDescriptor argument = (ArgumentDescriptor)serviceDescriptor.ImplementationInstance!;
            argument.Id.Should().Be("arg1");
            argument.DataType.Should().Be(DataType.CreditCard);
            argument.CustomDataType.Should().BeNull();
            argument.Description.Should().Be("description1");
            argument.Alias.Should().Be("arg-alias1");
            argument.DefaultValue.Should().Be(4444555544445555);
            argument.Required.Should().BeTrue();
            argument.Disabled.Should().BeFalse();
            argument.Obsolete.Should().BeTrue();
        }

        [Fact]
        public void AddArgument_Adds_Custom_DataType_Correctly()
        {
            IArgumentBuilder argumentBuilder = commandBuilder.AddArgument("arg1", "custom-dt", "description1", alias: null, defaultValue: null, required: false, disabled: true, obsolete: false);

            // Argument builder, command builder have different service collections.
            argumentBuilder.Services.Should().NotBeSameAs(commandBuilder.Services);

            ServiceDescriptor serviceDescriptor = argumentBuilder.Services.First(e => e.ServiceType.Equals(typeof(ArgumentDescriptor)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();

            ArgumentDescriptor argument = (ArgumentDescriptor)serviceDescriptor.ImplementationInstance!;
            argument.Id.Should().Be("arg1");
            argument.DataType.Should().Be(DataType.Custom);
            argument.CustomDataType.Should().Be("custom-dt");
            argument.Description.Should().Be("description1");
            argument.Alias.Should().BeNull();
            argument.DefaultValue.Should().BeNull();
            argument.Required.Should().BeFalse();
            argument.Disabled.Should().BeTrue();
            argument.Obsolete.Should().BeFalse();
        }

        private ICliBuilder cliBuilder = null!;
        private ICommandBuilder commandBuilder = null!;
    }
}
