/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Terminal.Extensions;
using PerpetualIntelligence.Terminal.Hosting;
using PerpetualIntelligence.Shared.Attributes.Validation;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Commands.Declarative
{
    public class DeclarativeTargetTests : IAsyncDisposable
    {
        public DeclarativeTargetTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
            cliBuilder = new(serviceCollection);
        }

        [Fact]
        public void Build_Should_Read_ArgumentValidation_Correctly()
        {
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget1>();
            ServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.OptionDescriptors.Should().NotBeNull();

            OptionDescriptor arg1 = cmd.OptionDescriptors!.First(e => e.Id.Equals("arg1"));
            arg1.ValueCheckers.Should().BeNull();

            OptionDescriptor arg2 = cmd.OptionDescriptors!.First(e => e.Id.Equals("arg2"));
            arg2.ValueCheckers.Should().NotBeNull();
            arg2.ValueCheckers!.Count().Should().Be(2);
            DataValidationOptionValueChecker val2Checker1 = (DataValidationOptionValueChecker)arg2.ValueCheckers!.First();
            val2Checker1.ValidationAttribute.Should().BeOfType<RequiredAttribute>();
            DataValidationOptionValueChecker val2Checker2 = (DataValidationOptionValueChecker)arg2.ValueCheckers!.Last();
            val2Checker2.ValidationAttribute.Should().BeOfType<OneOfAttribute>();
            OneOfAttribute val2OneOf = (OneOfAttribute)(val2Checker2.ValidationAttribute);
            val2OneOf.AllowedValues.Should().BeEquivalentTo(new string[] { "test1", "test2", "test3" });

            OptionDescriptor arg3 = cmd.OptionDescriptors!.First(e => e.Id.Equals("arg3"));
            arg3.ValueCheckers.Should().NotBeNull();
            arg3.ValueCheckers!.Count().Should().Be(1);
            DataValidationOptionValueChecker val1Checker3 = (DataValidationOptionValueChecker)arg3.ValueCheckers!.First();
            val1Checker3.ValidationAttribute.Should().BeOfType<RangeAttribute>();
            RangeAttribute val1Range = (RangeAttribute)(val1Checker3.ValidationAttribute);
            val1Range.Minimum.Should().Be(25.34);
            val1Range.Maximum.Should().Be(40.56);
        }

        [Fact]
        public void Build_Should_Read_NoOptionDescriptor_Correctly()
        {
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget5>();
            ServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.OptionDescriptors.Should().BeNull();
        }

        [Fact]
        public void Build_Should_Read_NoCommandTags_Correctly()
        {
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget5>();
            ServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.Tags.Should().BeNull();
        }

        [Fact]
        public void Build_Should_Read_CommandTags_Correctly()
        {
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget4>();
            ServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.Tags.Should().BeEquivalentTo(new string[] { "tag1", "tag2", "tag3" });
        }

        [Fact]
        public void Build_Should_Read_NoArgumentValidaiton_Correctly()
        {
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget4>();
            ServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.OptionDescriptors.Should().NotBeNull();

            OptionDescriptor arg1 = cmd.OptionDescriptors!.First(e => e.Id.Equals("arg1"));
            arg1.ValueCheckers.Should().BeNull();

            OptionDescriptor arg2 = cmd.OptionDescriptors!.First(e => e.Id.Equals("arg2"));
            arg2.ValueCheckers.Should().BeNull();

            OptionDescriptor arg3 = cmd.OptionDescriptors!.First(e => e.Id.Equals("arg3"));
            arg3.ValueCheckers.Should().BeNull();
        }

        [Fact]
        public void Build_Should_Read_ArgumentCustomProperties_Correctly()
        {
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget1>();
            ServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.OptionDescriptors.Should().NotBeNull();

            OptionDescriptor arg1 = cmd.OptionDescriptors!.First(e => e.Id.Equals("arg1"));
            arg1.CustomProperties.Should().NotBeNull();
            arg1.CustomProperties!.Count.Should().Be(3);
            arg1.CustomProperties["a1Key1"].Should().Be("a1Value1");
            arg1.CustomProperties["a1Key2"].Should().Be("a1Value2");
            arg1.CustomProperties["a1Key3"].Should().Be("a1Value3");

            OptionDescriptor arg2 = cmd.OptionDescriptors!.First(e => e.Id.Equals("arg2"));
            arg2.CustomProperties.Should().NotBeNull();
            arg2.CustomProperties!.Count.Should().Be(2);
            arg2.CustomProperties["a2Key1"].Should().Be("a2Value1");
            arg2.CustomProperties["a2Key2"].Should().Be("a2Value2");

            OptionDescriptor arg3 = cmd.OptionDescriptors!.First(e => e.Id.Equals("arg3"));
            arg3.CustomProperties.Should().BeNull();
        }

        [Fact]
        public void Build_Should_Read_NoArgumentCustomProperties_Correctly()
        {
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget4>();
            ServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.OptionDescriptors.Should().NotBeNull();

            OptionDescriptor arg1 = cmd.OptionDescriptors!.First(e => e.Id.Equals("arg1"));
            arg1.CustomProperties.Should().BeNull();

            OptionDescriptor arg2 = cmd.OptionDescriptors!.First(e => e.Id.Equals("arg2"));
            arg2.CustomProperties.Should().BeNull();

            OptionDescriptor arg3 = cmd.OptionDescriptors!.First(e => e.Id.Equals("arg3"));
            arg3.CustomProperties.Should().BeNull();
        }

        [Fact]
        public void Build_ShouldRead_Options_Correctly()
        {
            cliBuilder.AddDeclarativeTarget<MockDeclarativeTarget1>();
            ServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            cmdDescs.First().Prefix.Should().Be("test grp cmd1");
            cmdDescs.First().OptionDescriptors.Should().HaveCount(3);

            var argDescs = cmdDescs.First().OptionDescriptors!.ToArray();
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
            argDescs[0].ValueCheckers.Should().BeNull();
            argDescs[0].DefaultValue.Should().BeNull();

            argDescs[1].Id.Should().Be("arg2");
            argDescs[1].DataType.Should().Be(DataType.Text);
            argDescs[1].Description.Should().Be("test arg desc2");
            argDescs[1].CustomDataType.Should().BeNull();
            argDescs[1].Required.Should().BeTrue();
            argDescs[1].Obsolete.Should().BeFalse();
            argDescs[1].Disabled.Should().BeTrue();
            argDescs[1].Alias.Should().Be("arg2_alias");
            argDescs[1].CustomProperties.Should().NotBeNull();
            argDescs[1].CustomProperties!.Keys.Should().Equal(new string[] { "a2Key1", "a2Key2" });
            argDescs[1].CustomProperties!.Values.Should().Equal(new string[] { "a2Value1", "a2Value2" });
            argDescs[1].ValueCheckers.Should().NotBeNull();
            argDescs[1].ValueCheckers.Should().HaveCount(2);
            argDescs[1].ValueCheckers!.Cast<DataValidationOptionValueChecker>().First().ValidationAttribute.Should().BeOfType<RequiredAttribute>();
            argDescs[1].ValueCheckers!.Cast<DataValidationOptionValueChecker>().Last().ValidationAttribute.Should().BeOfType<OneOfAttribute>();
            argDescs[1].DefaultValue.Should().Be("arg1 default val");

            argDescs[2].Id.Should().Be("arg3");
            argDescs[2].DataType.Should().Be(DataType.Custom);
            argDescs[2].Description.Should().Be("test arg desc3");
            argDescs[2].CustomDataType.Should().Be(nameof(System.Double));
            argDescs[2].Required.Should().BeTrue();
            argDescs[2].Obsolete.Should().BeTrue();
            argDescs[2].Disabled.Should().BeFalse();
            argDescs[2].Alias.Should().BeNull();
            argDescs[2].CustomProperties.Should().BeNull();
            argDescs[2].ValueCheckers.Should().NotBeNull();
            argDescs[2].ValueCheckers.Should().HaveCount(1);
            argDescs[2].ValueCheckers!.Cast<DataValidationOptionValueChecker>().First().ValidationAttribute.Should().BeOfType<RangeAttribute>();
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
        public void Builder_Should_Read_NoCustomCommandProps_Correctly()
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
        public void TargetDoesNotImplements_IDeclarativeTarget()
        {
            AssemblyName aName = new AssemblyName("PiCliDeclarativeDynamicAssembly1");
            AssemblyBuilder ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndCollect);

            // The module name is usually the same as the assembly name.
            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name!);

            // Dynamic type with no IDeclarativeTarget
            var typeBuilder = mb.DefineType("TestMockNoTarget", TypeAttributes.Public, parent: null);
            Type mockType = typeBuilder.CreateType();

            // No target will be added as it does not implements IDeclarativeTarget
            cliBuilder.AddDeclarativeAssembly(mockType);
            ServiceProvider serviceProvider = cliBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().BeEmpty();
        }

        [Fact]
        public void TargetImplements_IDeclarativeTarget()
        {
            AssemblyName aName = new AssemblyName("PiCliDeclarativeDynamicAssembly2");
            AssemblyBuilder ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndCollect);

            // The module name is usually the same as the assembly name.
            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name!);

            // Dynamic type with IDeclarativeTarget
            var typeBuilder = mb.DefineType("TestMockNoTarget", TypeAttributes.Public, parent: null, interfaces: new Type[] { typeof(IDeclarativeTarget) });
            Type mockType = typeBuilder.CreateType();

            // This means that we tried adding the target as it implements IDeclarativeTarget
            Action act = () => cliBuilder.AddDeclarativeAssembly(mockType);
            act.Should().Throw<ErrorException>().WithMessage("The declarative target does not define command descriptor.");
        }

        private void ConfigureServicesDelegate(IServiceCollection arg2)
        {
            serviceCollection = arg2;
        }

        public ValueTask DisposeAsync()
        {
            host.Dispose();
            return ValueTask.CompletedTask;
        }

        private TerminalBuilder cliBuilder;
        private IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}