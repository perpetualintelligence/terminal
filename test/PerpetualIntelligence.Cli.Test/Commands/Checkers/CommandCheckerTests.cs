/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Mappers;
using PerpetualIntelligence.Cli.Commands.Stores;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Cli.Stores.InMemory;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    [TestClass]
    public class CommandCheckerTests : LogTest
    {
        public CommandCheckerTests() : base(TestLogger.Create<CommandCheckerTests>())
        {
        }

        [TestMethod]
        public async Task CommandDescriptorNoArgsShoutNotErrorAsync()
        {
            CommandDescriptor identity = new("id1", "name1", "prefix1");

            // During integration extractor filters the command unsupported args
            Command argsCommand = new(identity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key1", "value1", DataType.Text),
                new Argument("key2", "value2", DataType.Text)
            };

            CommandCheckerContext context = new(identity, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task DisabledArgumentShouldErrorAsync()
        {
            CommandDescriptor noArgsIdentity = new("id1", "name1", "prefix1");
            noArgsIdentity.ArgumentDescriptors = new()
            {
                new ArgumentDescriptor("key1", DataType.Text) { Disabled = true }
            };

            Command argsCommand = new(noArgsIdentity);
            argsCommand.Arguments = new()
            {
                new Argument("key1", "value1", DataType.Text)
            };

            CommandCheckerContext context = new(noArgsIdentity, argsCommand);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument is disabled. command_name=name1 command_id=id1 argument=key1");
        }

        [TestMethod]
        public async Task EnabledArgumentShouldNotErrorAsync()
        {
            CommandDescriptor noArgsIdentity = new("id1", "name1", "prefix1");
            noArgsIdentity.ArgumentDescriptors = new()
            {
                new("key1", DataType.Text)
                { Disabled = false }
            };

            Command argsCommand = new Command(noArgsIdentity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key1", "value1", DataType.Text)
            };

            CommandCheckerContext context = new(noArgsIdentity, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task InvalidValueValidShouldErrorAsync()
        {
            CommandDescriptor identity = new("id1", "name1", "prefix1");
            identity.ArgumentDescriptors = new ArgumentDescriptors()
            {
                new ArgumentDescriptor("key1", DataType.Text)
            };

            Command argsCommand = new Command(identity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key1", 36, DataType.Text)
            };

            CommandCheckerContext context = new(identity, argsCommand);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument value does not match the mapped type. argument=key1 type=System.String data_type=Text value_type=Int32 value=36");
        }

        [TestMethod]
        public async Task ObsoleteArgumentAndObsoleteAllowedShouldNotErrorAsync()
        {
            CommandDescriptor noArgsIdentity = new("id1", "name1", "prefix1");
            noArgsIdentity.ArgumentDescriptors = new ArgumentDescriptors()
            {
                new ArgumentDescriptor("key1", DataType.Text)
                {Obsolete = true}
            };

            Command argsCommand = new(noArgsIdentity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key1", "value1", DataType.Text)
            };

            CommandCheckerContext context = new(noArgsIdentity, argsCommand);

            options.Checker.AllowObsoleteArgument = true;
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ObsoleteArgumentAndObsoleteNotAllowedShouldErrorAsync()
        {
            CommandDescriptor noArgsIdentity = new("id1", "name1", "prefix1");
            noArgsIdentity.ArgumentDescriptors = new ArgumentDescriptors()
            {
                new ArgumentDescriptor("key1", DataType.Text)
                {Obsolete = true}
            };

            Command argsCommand = new(noArgsIdentity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key1", "value1", DataType.Text)
            };

            CommandCheckerContext context = new(noArgsIdentity, argsCommand);

            options.Checker.AllowObsoleteArgument = null;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument is obsolete. command_name=name1 command_id=id1 argument=key1");

            options.Checker.AllowObsoleteArgument = false;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument is obsolete. command_name=name1 command_id=id1 argument=key1");
        }

        [TestMethod]
        public async Task RequiredArgumentMissingShouldErrorAsync()
        {
            CommandDescriptor identity = new CommandDescriptor("id1", "name1", "prefix1");
            identity.ArgumentDescriptors = new ArgumentDescriptors()
            {
                new ArgumentDescriptor("key1", DataType.Text, required:true)
            };

            Command argsCommand = new Command(identity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key2", "value2", DataType.Text)
            };

            CommandCheckerContext context = new(identity, argsCommand);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.MissingArgument, "The required argument is missing. command_name=name1 command_id=id1 argument=key1");
        }

        [TestMethod]
        public async Task RequiredArgumentPassedShouldNotErrorAsync()
        {
            CommandDescriptor identity = new CommandDescriptor("id1", "name1", "prefix1");
            identity.ArgumentDescriptors = new ArgumentDescriptors()
            {
                new ArgumentDescriptor("key1", DataType.Text, required:true)
            };

            Command argsCommand = new Command(identity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key1", "value1", DataType.Text)
            };

            CommandCheckerContext context = new(identity, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ValueValidShouldNotErrorAsync()
        {
            CommandDescriptor identity = new("id1", "name1", "prefix1");
            identity.ArgumentDescriptors = new ArgumentDescriptors()
            {
                new ArgumentDescriptor("key1", DataType.Text)
            };

            Command argsCommand = new(identity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key1", "value1", DataType.Text)
            };

            CommandCheckerContext context = new(identity, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        protected override void OnTestInitialize()
        {
            options = MockCliOptions.New();
            mapper = new DataAnnotationsArgumentDataTypeMapper(options, TestLogger.Create<DataAnnotationsArgumentDataTypeMapper>());
            valueChecker = new ArgumentChecker(mapper, options, TestLogger.Create<ArgumentChecker>());
            checker = new CommandChecker(valueChecker, options, TestLogger.Create<CommandChecker>());
            commands = new InMemoryCommandDescriptorStore(MockCommands.Commands, options, TestLogger.Create<InMemoryCommandDescriptorStore>());
        }

        private CommandDescriptor NewCustomDataTypeCmdIdentity(string customDataType)
        {
            CommandDescriptor identity = new("id1", "name1", "prefix1");
            identity.ArgumentDescriptors = new ArgumentDescriptors()
            {
                new ArgumentDescriptor("key1", customDataType)
            };
            return identity;
        }

        private CommandChecker checker = null!;
        private ICommandDescriptorStore commands = null!;
        private IArgumentDataTypeMapper mapper = null!;
        private CliOptions options = null!;
        private IArgumentChecker valueChecker = null!;
    }
}
