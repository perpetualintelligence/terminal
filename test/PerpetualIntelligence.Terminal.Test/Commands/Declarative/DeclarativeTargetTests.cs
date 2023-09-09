﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Shared.Attributes.Validation;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Terminal.Extensions;
using PerpetualIntelligence.Terminal.Hosting;
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
            terminalBuilder = new(serviceCollection);
        }

        [Fact]
        public void Build_Should_Read_ArgumentValidation_Correctly()
        {
            terminalBuilder.AddDeclarativeTarget<MockDeclarativeTarget1>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.OptionDescriptors.Should().NotBeNull();

            OptionDescriptor opt1 = cmd.OptionDescriptors!.First(e => e.Id.Equals("opt1"));
            opt1.ValueCheckers.Should().BeNull();

            OptionDescriptor opt2 = cmd.OptionDescriptors!.First(e => e.Id.Equals("opt2"));
            opt2.ValueCheckers.Should().NotBeNull();
            opt2.ValueCheckers!.Count().Should().Be(2);
            DataValidationOptionValueChecker val2Checker1 = (DataValidationOptionValueChecker)opt2.ValueCheckers!.First();
            val2Checker1.ValidationAttribute.Should().BeOfType<RequiredAttribute>();
            DataValidationOptionValueChecker val2Checker2 = (DataValidationOptionValueChecker)opt2.ValueCheckers!.Last();
            val2Checker2.ValidationAttribute.Should().BeOfType<OneOfAttribute>();
            OneOfAttribute val2OneOf = (OneOfAttribute)val2Checker2.ValidationAttribute;
            val2OneOf.AllowedValues.Should().BeEquivalentTo(new string[] { "test1", "test2", "test3" });

            OptionDescriptor opt3 = cmd.OptionDescriptors!.First(e => e.Id.Equals("opt3"));
            opt3.ValueCheckers.Should().NotBeNull();
            opt3.ValueCheckers!.Count().Should().Be(1);
            DataValidationOptionValueChecker val1Checker3 = (DataValidationOptionValueChecker)opt3.ValueCheckers!.First();
            val1Checker3.ValidationAttribute.Should().BeOfType<RangeAttribute>();
            RangeAttribute val1Range = (RangeAttribute)(val1Checker3.ValidationAttribute);
            val1Range.Minimum.Should().Be(25.34);
            val1Range.Maximum.Should().Be(40.56);
        }

        [Fact]
        public void Build_Should_Read_NoOptionDescriptor_Correctly()
        {
            terminalBuilder.AddDeclarativeTarget<MockDeclarativeTarget5>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.OptionDescriptors.Should().BeNull();
        }

        [Fact]
        public void Build_Should_Read_NoCommandTags_Correctly()
        {
            terminalBuilder.AddDeclarativeTarget<MockDeclarativeTarget5>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.Tags.Should().BeNull();
        }

        [Fact]
        public void Build_Should_Read_CommandTags_Correctly()
        {
            terminalBuilder.AddDeclarativeTarget<MockDeclarativeTarget4>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.Tags.Should().BeEquivalentTo(new string[] { "tag1", "tag2", "tag3" });
        }

        [Fact]
        public void Build_Should_Read_NoArgumentValidation_Correctly()
        {
            terminalBuilder.AddDeclarativeTarget<MockDeclarativeTarget4>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            CommandDescriptor cmd = cmdDescs.First();
            cmd.OptionDescriptors.Should().NotBeNull();

            OptionDescriptor opt1 = cmd.OptionDescriptors!.First(e => e.Id.Equals("opt1"));
            opt1.ValueCheckers.Should().BeNull();

            OptionDescriptor opt2 = cmd.OptionDescriptors!.First(e => e.Id.Equals("opt2"));
            opt2.ValueCheckers.Should().BeNull();

            OptionDescriptor opt3 = cmd.OptionDescriptors!.First(e => e.Id.Equals("opt3"));
            opt3.ValueCheckers.Should().BeNull();
        }

        [Fact]
        public void Build_ShouldRead_Options_Correctly()
        {
            terminalBuilder.AddDeclarativeTarget<MockDeclarativeTarget1>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            cmdDescs.First().OptionDescriptors.Should().HaveCount(3);

            var optDescs = cmdDescs.First().OptionDescriptors!.ToArray();
            optDescs[0].Id.Should().Be("opt1");
            optDescs[0].DataType.Should().Be(nameof(String));
            optDescs[0].Description.Should().Be("test arg desc1");
            optDescs[0].DataType.Should().BeNull();
            optDescs[0].Flags.Should().Be(OptionFlags.None);
            optDescs[0].Alias.Should().BeNull();
            optDescs[0].ValueCheckers.Should().BeNull();

            optDescs[1].Id.Should().Be("opt2");
            optDescs[1].DataType.Should().Be(nameof(String));
            optDescs[1].Description.Should().Be("test arg desc2");
            optDescs[1].DataType.Should().BeNull();
            optDescs[1].Flags.Should().Be(OptionFlags.Required | OptionFlags.Disabled);
            optDescs[1].Alias.Should().Be("opt2_alias");
            optDescs[1].ValueCheckers.Should().NotBeNull();
            optDescs[1].ValueCheckers.Should().HaveCount(2);
            optDescs[1].ValueCheckers!.Cast<DataValidationOptionValueChecker>().First().ValidationAttribute.Should().BeOfType<RequiredAttribute>();
            optDescs[1].ValueCheckers!.Cast<DataValidationOptionValueChecker>().Last().ValidationAttribute.Should().BeOfType<OneOfAttribute>();

            optDescs[2].Id.Should().Be("opt3");
            optDescs[2].Description.Should().Be("test arg desc3");
            optDescs[2].DataType.Should().Be(nameof(Double));
            optDescs[2].Flags.Should().Be(OptionFlags.Required | OptionFlags.Obsolete);
            optDescs[2].Alias.Should().BeNull();
            optDescs[2].ValueCheckers.Should().NotBeNull();
            optDescs[2].ValueCheckers.Should().HaveCount(1);
            optDescs[2].ValueCheckers!.Cast<DataValidationOptionValueChecker>().First().ValidationAttribute.Should().BeOfType<RangeAttribute>();
        }

        [Fact]
        public void Builder_ShouldAdd_Descriptors_To_ServiceCollection()
        {
            terminalBuilder.AddDeclarativeTarget<MockDeclarativeTarget1>();
            terminalBuilder.AddDeclarativeTarget<MockDeclarativeTarget2>();
            terminalBuilder.AddDeclarativeTarget<MockDeclarativeTarget3>();

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
            terminalBuilder.AddDeclarativeTarget<MockDeclarativeTarget1>();
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
            terminalBuilder.AddDeclarativeTarget<MockDeclarativeTarget2>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            cmdDescs.First().CustomProperties.Should().BeNull();
        }

        [Fact]
        public void Builder_ShouldRead_NoTags_Correctly()
        {
            terminalBuilder.AddDeclarativeTarget<MockDeclarativeTarget2>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            cmdDescs.First().Tags.Should().BeNull();
        }

        [Fact]
        public void Builder_ShouldRead_Tags_Correctly()
        {
            terminalBuilder.AddDeclarativeTarget<MockDeclarativeTarget1>();
            ServiceProvider serviceProvider = terminalBuilder.Services.BuildServiceProvider();
            var cmdDescs = serviceProvider.GetServices<CommandDescriptor>();
            cmdDescs.Should().HaveCount(1);

            cmdDescs.First().Tags.Should().Equal(new string[] { "tag1", "tag2", "tag3" });
        }

        [Fact]
        public void TargetMustDefine_CommandChecker()
        {
            Action act = () => terminalBuilder.AddDeclarativeTarget<MockDeclarativeTargetNoCommandChecker>();
            act.Should().Throw<ErrorException>().WithMessage("The declarative target does not define command checker.");
        }

        [Fact]
        public void TargetMustDefine_CommandDescriptor()
        {
            Action act = () => terminalBuilder.AddDeclarativeTarget<MockDeclarativeTargetNoCommandDescriptor>();
            act.Should().Throw<ErrorException>().WithMessage("The declarative target does not define command descriptor.");
        }

        [Fact]
        public void TargetMustDefine_CommandRunner()
        {
            Action act = () => terminalBuilder.AddDeclarativeTarget<MockDeclarativeTargetNoCommandRunner>();
            act.Should().Throw<ErrorException>().WithMessage("The declarative target does not define command runner.");
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
            var typeBuilder = mb.DefineType("TestMockNoTarget", TypeAttributes.Public, parent: null, interfaces: new Type[] { typeof(IDeclarativeTarget) });
            Type mockType = typeBuilder.CreateType();

            // This means that we tried adding the target as it implements IDeclarativeTarget
            Action act = () => terminalBuilder.AddDeclarativeAssembly(mockType);
            act.Should().Throw<ErrorException>().WithMessage("The declarative target does not define command descriptor.");
        }

        private void ConfigureServicesDelegate(IServiceCollection opt2)
        {
            serviceCollection = opt2;
        }

        public ValueTask DisposeAsync()
        {
            host.Dispose();
            return ValueTask.CompletedTask;
        }

        private readonly TerminalBuilder terminalBuilder;
        private readonly IHost host = null!;
        private IServiceCollection serviceCollection = null!;
    }
}