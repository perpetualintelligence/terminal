/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneImlx.Shared.Attributes.Validation;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Declarative
{
    public class DeclarativeRunnerTests : IAsyncDisposable
    {
        public DeclarativeRunnerTests()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDelegate);
            host = hostBuilder.Build();
            terminalBuilder = new(serviceCollection, new TerminalAsciiTextHandler());
        }

        [Fact]
        public void No_CommandOwner_Throws()
        {
            Action act = () => terminalBuilder.AddDeclarativeRunner<MockDeclarativeTargetNoCommandOwnerRunner>();
            act.Should().Throw<TerminalException>().WithMessage("The declarative target does not define command owner.");
        }

        [Fact]
        public void Build_Should_Read_OptionValidation_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunner1>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.OptionDescriptors.Should().NotBeNull();

            OptionDescriptor opt1 = cmd.OptionDescriptors!["opt1"];
            opt1.ValueCheckers.Should().BeNull();

            OptionDescriptor opt2 = cmd.OptionDescriptors["opt2"];
            opt2.ValueCheckers.Should().NotBeNull();
            opt2.ValueCheckers!.Count().Should().Be(2);
            DataValidationValueChecker<Option> val2Checker1 = (DataValidationValueChecker<Option>)opt2.ValueCheckers!.First();
            val2Checker1.ValidationAttribute.Should().BeOfType<RequiredAttribute>();
            DataValidationValueChecker<Option> val2Checker2 = (DataValidationValueChecker<Option>)opt2.ValueCheckers!.Last();
            val2Checker2.ValidationAttribute.Should().BeOfType<OneOfAttribute>();
            OneOfAttribute val2OneOf = (OneOfAttribute)val2Checker2.ValidationAttribute;
            val2OneOf.AllowedValues.Should().BeEquivalentTo(new string[] { "test1", "test2", "test3" });

            OptionDescriptor opt3 = cmd.OptionDescriptors["opt3"];
            opt3.ValueCheckers.Should().NotBeNull();
            opt3.ValueCheckers!.Count().Should().Be(1);
            DataValidationValueChecker<Option> val1Checker3 = (DataValidationValueChecker<Option>)opt3.ValueCheckers!.First();
            val1Checker3.ValidationAttribute.Should().BeOfType<RangeAttribute>();
            RangeAttribute val1Range = (RangeAttribute)val1Checker3.ValidationAttribute;
            val1Range.Minimum.Should().Be(25.34);
            val1Range.Maximum.Should().Be(40.56);
        }

        [Fact]
        public void Build_Should_Read_ArgumentValidation_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunner1>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.ArgumentDescriptors.Should().NotBeNull();

            ArgumentDescriptor arg1 = cmd.ArgumentDescriptors!["arg1"];
            arg1.ValueCheckers.Should().BeNull();

            ArgumentDescriptor arg2 = cmd.ArgumentDescriptors["arg2"];
            arg2.ValueCheckers.Should().NotBeNull();
            arg2.ValueCheckers!.Count().Should().Be(2);
            DataValidationValueChecker<Argument> val2Checker1 = (DataValidationValueChecker<Argument>)arg2.ValueCheckers!.First();
            val2Checker1.ValidationAttribute.Should().BeOfType<RequiredAttribute>();
            DataValidationValueChecker<Argument> val2Checker2 = (DataValidationValueChecker<Argument>)arg2.ValueCheckers!.Last();
            val2Checker2.ValidationAttribute.Should().BeOfType<OneOfAttribute>();
            OneOfAttribute val2OneOf = (OneOfAttribute)val2Checker2.ValidationAttribute;
            val2OneOf.AllowedValues.Should().BeEquivalentTo(new string[] { "test1", "test2", "test3" });

            ArgumentDescriptor arg3 = cmd.ArgumentDescriptors["arg3"];
            arg3.ValueCheckers.Should().NotBeNull();
            arg3.ValueCheckers!.Count().Should().Be(1);
            DataValidationValueChecker<Argument> val1Checker3 = (DataValidationValueChecker<Argument>)arg3.ValueCheckers!.First();
            val1Checker3.ValidationAttribute.Should().BeOfType<RangeAttribute>();
            RangeAttribute val1Range = (RangeAttribute)val1Checker3.ValidationAttribute;
            val1Range.Minimum.Should().Be(25.34);
            val1Range.Maximum.Should().Be(40.56);
        }

        [Fact]
        public void Build_Should_Read_NoValueDescriptor_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunner5>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.OptionDescriptors.Should().BeNull();
            cmd.ArgumentDescriptors.Should().BeNull();
        }

        [Fact]
        public void Build_Should_Read_NoCommandTags_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunner5>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.TagIds.Should().BeNull();
        }

        [Fact]
        public void Build_Should_Read_CommandTags_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunner4>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.TagIds.Should().BeEquivalentTo(new string[] { "tag1", "tag2", "tag3" });
        }

        [Fact]
        public void Build_Should_Read_NoValidation_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunner4>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();

            // Arguments
            cmd.ArgumentDescriptors.Should().NotBeNull();

            ArgumentDescriptor arg1 = cmd.ArgumentDescriptors!["arg1"];
            arg1.ValueCheckers.Should().BeNull();

            ArgumentDescriptor arg2 = cmd.ArgumentDescriptors["arg2"];
            arg2.ValueCheckers.Should().BeNull();

            ArgumentDescriptor arg3 = cmd.ArgumentDescriptors["arg3"];
            arg3.ValueCheckers.Should().BeNull();

            // Options
            cmd.OptionDescriptors.Should().NotBeNull();

            OptionDescriptor opt1 = cmd.OptionDescriptors!["opt1"];
            opt1.ValueCheckers.Should().BeNull();

            OptionDescriptor opt2 = cmd.OptionDescriptors["opt2"];
            opt2.ValueCheckers.Should().BeNull();

            OptionDescriptor opt3 = cmd.OptionDescriptors["opt3"];
            opt3.ValueCheckers.Should().BeNull();
        }

        [Fact]
        public void Build_ShouldRead_Arguments_And_Options_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunner1>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            // Arguments
            cmdDescs.First().ArgumentDescriptors.Should().HaveCount(3);
            var argDescs = cmdDescs.First().ArgumentDescriptors;
            argDescs.Should().NotBeNull();

            argDescs!["arg1"].Id.Should().Be("arg1");
            argDescs["arg1"].Order.Should().Be(1);
            argDescs["arg1"].DataType.Should().Be(nameof(String));
            argDescs["arg1"].Description.Should().Be("test arg desc1");
            argDescs["arg1"].Flags.Should().Be(ArgumentFlags.None);
            argDescs["arg1"].ValueCheckers.Should().BeNull();

            argDescs["arg2"].Id.Should().Be("arg2");
            argDescs["arg2"].Order.Should().Be(2);
            argDescs["arg2"].DataType.Should().Be(nameof(String));
            argDescs["arg2"].Description.Should().Be("test arg desc2");
            argDescs["arg2"].Flags.Should().Be(ArgumentFlags.Required | ArgumentFlags.Disabled);
            argDescs["arg2"].ValueCheckers.Should().NotBeNull();
            argDescs["arg2"].ValueCheckers.Should().HaveCount(2);
            argDescs["arg2"].ValueCheckers!.Cast<DataValidationValueChecker<Argument>>().First().ValidationAttribute.Should().BeOfType<RequiredAttribute>();
            argDescs["arg2"].ValueCheckers!.Cast<DataValidationValueChecker<Argument>>().Last().ValidationAttribute.Should().BeOfType<OneOfAttribute>();

            argDescs["arg3"].Id.Should().Be("arg3");
            argDescs["arg3"].Order.Should().Be(3);
            argDescs["arg3"].DataType.Should().Be(nameof(Double));
            argDescs["arg3"].Description.Should().Be("test arg desc3");
            argDescs["arg3"].Flags.Should().Be(ArgumentFlags.Required | ArgumentFlags.Obsolete);
            argDescs["arg3"].ValueCheckers.Should().NotBeNull();
            argDescs["arg3"].ValueCheckers.Should().HaveCount(1);
            argDescs["arg3"].ValueCheckers!.Cast<DataValidationValueChecker<Argument>>().First().ValidationAttribute.Should().BeOfType<RangeAttribute>();

            // Options
            cmdDescs.First().OptionDescriptors.Should().HaveCount(4);
            var optDescs = cmdDescs.First().OptionDescriptors;
            optDescs.Should().NotBeNull();

            optDescs!["opt1"].Id.Should().Be("opt1");
            optDescs["opt1"].DataType.Should().Be(nameof(String));
            optDescs["opt1"].Description.Should().Be("test opt desc1");
            optDescs["opt1"].Flags.Should().Be(OptionFlags.None);
            optDescs["opt1"].Alias.Should().BeNull();
            optDescs["opt1"].ValueCheckers.Should().BeNull();

            optDescs["opt2"].Id.Should().Be("opt2");
            optDescs["opt2"].DataType.Should().Be(nameof(String));
            optDescs["opt2"].Description.Should().Be("test opt desc2");
            optDescs["opt2"].Flags.Should().Be(OptionFlags.Required | OptionFlags.Disabled);
            optDescs["opt2"].Alias.Should().Be("opt2_alias");
            optDescs["opt2"].ValueCheckers.Should().NotBeNull();
            optDescs["opt2"].ValueCheckers.Should().HaveCount(2);
            optDescs["opt2"].ValueCheckers!.Cast<DataValidationValueChecker<Option>>().First().ValidationAttribute.Should().BeOfType<RequiredAttribute>();
            optDescs["opt2"].ValueCheckers!.Cast<DataValidationValueChecker<Option>>().Last().ValidationAttribute.Should().BeOfType<OneOfAttribute>();

            optDescs["opt2_alias"].Id.Should().Be("opt2");
            optDescs["opt2_alias"].DataType.Should().Be(nameof(String));
            optDescs["opt2_alias"].Description.Should().Be("test opt desc2");
            optDescs["opt2_alias"].Flags.Should().Be(OptionFlags.Required | OptionFlags.Disabled);
            optDescs["opt2_alias"].Alias.Should().Be("opt2_alias");
            optDescs["opt2_alias"].ValueCheckers.Should().NotBeNull();
            optDescs["opt2_alias"].ValueCheckers.Should().HaveCount(2);
            optDescs["opt2_alias"].ValueCheckers!.Cast<DataValidationValueChecker<Option>>().First().ValidationAttribute.Should().BeOfType<RequiredAttribute>();
            optDescs["opt2_alias"].ValueCheckers!.Cast<DataValidationValueChecker<Option>>().Last().ValidationAttribute.Should().BeOfType<OneOfAttribute>();

            optDescs["opt3"].Id.Should().Be("opt3");
            optDescs["opt3"].Description.Should().Be("test opt desc3");
            optDescs["opt3"].DataType.Should().Be(nameof(Double));
            optDescs["opt3"].Flags.Should().Be(OptionFlags.Required | OptionFlags.Obsolete);
            optDescs["opt3"].Alias.Should().BeNull();
            optDescs["opt3"].ValueCheckers.Should().NotBeNull();
            optDescs["opt3"].ValueCheckers.Should().HaveCount(1);
            optDescs["opt3"].ValueCheckers!.Cast<DataValidationValueChecker<Option>>().First().ValidationAttribute.Should().BeOfType<RangeAttribute>();
        }

        [Fact]
        public void Builder_ShouldAdd_Descriptors_To_ServiceCollection()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunner1>();
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunner2>();
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunner3>();

            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(3);

            cmdDescs.ToArray()[0].Id.Should().Be("id1");
            cmdDescs.ToArray()[0].Name.Should().Be("name1");

            cmdDescs.ToArray()[1].Id.Should().Be("id2");
            cmdDescs.ToArray()[1].Name.Should().Be("name2");

            cmdDescs.ToArray()[2].Id.Should().Be("id3");
            cmdDescs.ToArray()[2].Name.Should().Be("name3");
        }

        [Fact]
        public void Builder_ShouldRead_CustomCommandProps_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunner1>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            cmdDescs.First().CustomProperties.Should().NotBeNull();
            cmdDescs.First().CustomProperties!.Keys.Should().Equal(new string[] { "key1", "key2", "key3" });
            cmdDescs.First().CustomProperties!.Values.Should().Equal(new string[] { "value1", "value2", "value3" });
        }

        [Fact]
        public void Builder_Should_Read_NoCustomCommandProps_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunner2>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            cmdDescs.First().CustomProperties.Should().BeNull();
        }

        [Fact]
        public void Builder_ShouldRead_NoTags_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunner2>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            cmdDescs.First().TagIds.Should().BeNull();
        }

        [Fact]
        public void Builder_ShouldRead_Tags_Correctly()
        {
            terminalBuilder.AddDeclarativeRunner<MockDeclarativeRunner1>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            cmdDescs.First().TagIds.Should().Equal(new string[] { "tag1", "tag2", "tag3" });
        }

        [Fact]
        public void TargetMustDefine_CommandChecker()
        {
            Action act = () => terminalBuilder.AddDeclarativeRunner<MockDeclarativeTargetNoCommandCheckerRunner>();
            act.Should().Throw<TerminalException>().WithMessage("The declarative target does not define command checker.");
        }

        [Fact]
        public void TargetMustDefine_CommandDescriptor()
        {
            Action act = () => terminalBuilder.AddDeclarativeRunner<MockDeclarativeTargetNoCommandDescriptorRunner>();
            act.Should().Throw<TerminalException>().WithMessage("The declarative target does not define command descriptor.");
        }

        [Fact]
        public void TargetDoesNotImplements_IDeclarativeTarget()
        {
            AssemblyName aName = new("PiCliDeclarativeDynamicAssembly1");
            AssemblyBuilder ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndCollect);

            // The module name is usually the same as the assembly name.
            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name!);

            // Dynamic type with no IDeclarativeTarget
            var typeBuilder = mb.DefineType("TestMockNoTarget", TypeAttributes.Public, parent: null);
            Type mockType = typeBuilder.CreateType();

            // No target will be added as it does not implements IDeclarativeTarget
            terminalBuilder.AddDeclarativeAssembly(mockType);
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().BeEmpty();
        }

        [Fact]
        public void TargetImplements_IDeclarativeTarget()
        {
            AssemblyName aName = new("PiCliDeclarativeDynamicAssembly2");
            AssemblyBuilder ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndCollect);

            // The module name is usually the same as the assembly name.
            ModuleBuilder mb = ab.DefineDynamicModule(aName.Name!);

            // Dynamic type with IDeclarativeTarget
            var typeBuilder = mb.DefineType("TestMockNoTarget", TypeAttributes.Public, parent: null, interfaces: new Type[] { typeof(IDeclarativeRunner) });
            Type mockType = typeBuilder.CreateType();

            // This means that we tried adding the target as it implements IDeclarativeTarget
            Action act = () => terminalBuilder.AddDeclarativeAssembly(mockType);
            act.Should().Throw<TerminalException>().WithMessage("The declarative target does not define command descriptor.");
        }

        private void ConfigureServicesDelegate(IServiceCollection opt2)
        {
            serviceCollection = opt2;
        }

        public ValueTask DisposeAsync()
        {
            host.Dispose();
            return new ValueTask(Task.CompletedTask);
        }

        private readonly TerminalBuilder terminalBuilder;
        private readonly IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}