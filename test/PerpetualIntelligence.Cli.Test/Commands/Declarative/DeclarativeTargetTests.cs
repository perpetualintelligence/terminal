/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Cli.Extensions;
using PerpetualIntelligence.Cli.Integration;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace PerpetualIntelligence.Cli.Commands.Declarative
{
    public class DeclarativeTargetTests : IDisposable
    {
        public DeclarativeTargetTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
            cliBuilder = new(serviceCollection);
        }

        [Fact]
        public void Build_ShouldRead_Arguments_Correctly()
        {
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget1>();
            ServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            cmdDescs.First().Prefix.Should().Be("test grp cmd1");
            cmdDescs.First().ArgumentDescriptors.Should().HaveCount(3);

            var argDescs = cmdDescs.First().ArgumentDescriptors!.ToArray();
            argDescs[0].Id.Should().Be("arg1");
            argDescs[0].DataType.Should().Be(DataType.Text);
            argDescs[0].Description.Should().Be("test arg desc1");
            argDescs[0].CustomDataType.Should().BeNull();
            argDescs[0].Required.Should().BeFalse();
            argDescs[0].Obsolete.Should().BeFalse();
            argDescs[0].Disabled.Should().BeFalse();
            argDescs[0].Alias.Should().BeNull();
            argDescs[0].CustomProperties.Should().NotBeNull();
            argDescs[0].CustomProperties!.Keys.Should().Equal(new string[] { "a1Key1", "a1Key2", "a1Key3" });
            argDescs[0].CustomProperties!.Values.Should().Equal(new string[] { "a1Value1", "a1Value2", "a1Value3" });
            argDescs[0].ValidationAttributes.Should().BeNull();
            argDescs[0].DefaultValue.Should().BeNull();

            argDescs[1].Id.Should().Be("arg2");
            argDescs[1].DataType.Should().Be(DataType.PhoneNumber);
            argDescs[1].Description.Should().Be("test arg desc2");
            argDescs[1].CustomDataType.Should().BeNull();
            argDescs[1].Required.Should().BeTrue();
            argDescs[1].Obsolete.Should().BeFalse();
            argDescs[1].Disabled.Should().BeTrue();
            argDescs[1].Alias.Should().Be("arg2_alias");
            argDescs[1].CustomProperties.Should().NotBeNull();
            argDescs[1].CustomProperties!.Keys.Should().Equal(new string[] { "a2Key1", "a2Key2" });
            argDescs[1].CustomProperties!.Values.Should().Equal(new string[] { "a2Value1", "a2Value2" });
            argDescs[1].ValidationAttributes.Should().NotBeNull();
            argDescs[1].ValidationAttributes.Should().HaveCount(1);
            argDescs[1].ValidationAttributes!.First().Should().BeOfType<RequiredAttribute>();
            argDescs[1].DefaultValue.Should().Be(1111111111);

            argDescs[2].Id.Should().Be("arg3");
            argDescs[2].DataType.Should().Be(DataType.Custom);
            argDescs[2].Description.Should().Be("test arg desc3");
            argDescs[2].CustomDataType.Should().Be(nameof(System.Double));
            argDescs[2].Required.Should().BeTrue();
            argDescs[2].Obsolete.Should().BeTrue();
            argDescs[2].Disabled.Should().BeFalse();
            argDescs[2].Alias.Should().BeNull();
            argDescs[2].CustomProperties.Should().BeNull();
            argDescs[2].ValidationAttributes.Should().NotBeNull();
            argDescs[2].ValidationAttributes.Should().HaveCount(1);
            argDescs[2].ValidationAttributes!.First().Should().BeOfType<RangeAttribute>();
            argDescs[2].DefaultValue.Should().BeNull();
        }

        [Fact]
        public void Builder_ShouldAdd_Descriptors_To_ServiceCollection()
        {
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget1>();
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget2>();
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget3>();

            ServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(3);

            cmdDescs.ToArray()[0].Id.Should().Be("id1");
            cmdDescs.ToArray()[0].Name.Should().Be("name1");
            cmdDescs.ToArray()[0].Prefix.Should().Be("test grp cmd1");

            cmdDescs.ToArray()[1].Id.Should().Be("id2");
            cmdDescs.ToArray()[1].Name.Should().Be("name2");
            cmdDescs.ToArray()[1].Prefix.Should().Be("test grp cmd2");

            cmdDescs.ToArray()[2].Id.Should().Be("id3");
            cmdDescs.ToArray()[2].Name.Should().Be("name3");
            cmdDescs.ToArray()[2].Prefix.Should().Be("test grp cmd3");
        }

        [Fact]
        public void Builder_ShouldRead_CustomCommandProps_Correctly()
        {
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget1>();
            ServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            cmdDescs.First().Prefix.Should().Be("test grp cmd1");
            cmdDescs.First().CustomProperties.Should().NotBeNull();
            cmdDescs.First().CustomProperties!.Keys.Should().Equal(new string[] { "key1", "key2", "key3" });
            cmdDescs.First().CustomProperties!.Values.Should().Equal(new string[] { "value1", "value2", "value3" });
        }

        [Fact]
        public void Builder_ShouldRead_NoCustomCommandProps_Correctly()
        {
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget2>();
            ServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            cmdDescs.First().Prefix.Should().Be("test grp cmd2");
            cmdDescs.First().CustomProperties.Should().BeNull();
        }

        [Fact]
        public void Builder_ShouldRead_NoTags_Correctly()
        {
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget2>();
            ServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            cmdDescs.First().Prefix.Should().Be("test grp cmd2");
            cmdDescs.First().Tags.Should().BeNull();
        }

        [Fact]
        public void Builder_ShouldRead_Tags_Correctly()
        {
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget1>();
            ServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            cmdDescs.First().Prefix.Should().Be("test grp cmd1");
            cmdDescs.First().Tags.Should().Equal(new string[] { "tag1", "tag2", "tag3" });
        }

        public void Dispose()
        {
            host.Dispose();
        }

        [Fact]
        public void TargetMustDefine_CommandChecker()
        {
            Action act = () => cliBuilder.AddDeclarativeTarget<MockDeclarativeTargetNoCommandChecker>();
            act.Should().Throw<ErrorException>().WithMessage("The declarative target does not define command checker.");
        }

        [Fact]
        public void TargetMustDefine_CommandDescriptor()
        {
            Action act = () => cliBuilder.AddDeclarativeTarget<MockDeclarativeTargetNoCommandDescriptor>();
            act.Should().Throw<ErrorException>().WithMessage("The declarative target does not define command descriptor.");
        }

        [Fact]
        public void TargetMustDefine_CommandRunner()
        {
            Action act = () => cliBuilder.AddDeclarativeTarget<MockDeclarativeTargetNoCommandRunner>();
            act.Should().Throw<ErrorException>().WithMessage("The declarative target does not define command runner.");
        }

        [Fact]
        public void TargetMustDefine_TextHandler()
        {
            Action act = () => cliBuilder.AddDeclarativeTarget<MockDeclarativeTargetNoTarget>();
            act.Should().Throw<ErrorException>().WithMessage("The declarative target does not define text handler.");
        }

        private void ConfigureServicesDelegate(IServiceCollection arg2)
        {
            serviceCollection = arg2;
        }

        private CliBuilder cliBuilder;
        private IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}
