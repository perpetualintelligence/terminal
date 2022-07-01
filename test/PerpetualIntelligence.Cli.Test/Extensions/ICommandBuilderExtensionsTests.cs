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
        public void AddArgument_Adds_Correctly()
        {
            commandBuilder.AddArgument("arg1", DataType.CreditCard, "description1", required: true, defaultValue: 4444444444444444);

            ServiceDescriptor serviceDescriptor = commandBuilder.Services.First(e => e.ServiceType.Equals(typeof(ArgumentDescriptor)));
            serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
            serviceDescriptor.ImplementationType.Should().BeNull();

            ArgumentDescriptor argument = (ArgumentDescriptor)serviceDescriptor.ImplementationInstance!;
            argument.Id.Should().Be("arg1");
            argument.DataType.Should().Be(DataType.CreditCard);
            argument.Required.Should().Be(true);
            argument.Description.Should().Be("description1");
            argument.DefaultValue.Should().Be(4444444444444444);
        }

        private ICliBuilder cliBuilder = null!;
        private ICommandBuilder commandBuilder = null!;
    }
}
