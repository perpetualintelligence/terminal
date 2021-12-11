/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
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
    public class CommandCheckerTests : OneImlxLogTest
    {
        public CommandCheckerTests() : base(TestLogger.Create<CommandCheckerTests>())
        {
        }

        [TestMethod]
        public async Task CommandIdentityNoArgsShoutNotErrorAsync()
        {
            CommandIdentity identity = new("id1", "name1", "prefix1");

            // During integration extractor filters the command unsupported args
            Command argsCommand = new(identity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key1", "value1", DataType.Text),
                new Argument("key2", "value2", DataType.Text)
            };

            CommandCheckerContext context = new CommandCheckerContext(identity, argsCommand);
            var result = await checker.CheckAsync(context);
            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        public async Task DisabledArgumentShouldErrorAsync()
        {
            CommandIdentity noArgsIdentity = new("id1", "name1", "prefix1");
            noArgsIdentity.ArgumentIdentities = new()
            {
                new ArgumentIdentity("key1", DataType.Text) { Disabled = true }
            };

            Command argsCommand = new(noArgsIdentity);
            argsCommand.Arguments = new()
            {
                new Argument("key1", "value1", DataType.Text)
            };

            CommandCheckerContext context = new(noArgsIdentity, argsCommand);
            var result = await checker.CheckAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument is disabled. command_name=name1 command_id=id1 argument=key1");
        }

        [TestMethod]
        public async Task EnabledArgumentShouldNotErrorAsync()
        {
            CommandIdentity noArgsIdentity = new("id1", "name1", "prefix1");
            noArgsIdentity.ArgumentIdentities = new()
            {
                new("key1", DataType.Text)
                { Disabled = false }
            };

            Command argsCommand = new Command(noArgsIdentity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key1", "value1", DataType.Text)
            };

            CommandCheckerContext context = new CommandCheckerContext(noArgsIdentity, argsCommand);
            var result = await checker.CheckAsync(context);
            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        public async Task InvalidValueValidShouldErrorAsync()
        {
            CommandIdentity identity = new CommandIdentity("id1", "name1", "prefix1");
            identity.ArgumentIdentities = new ArgumentIdentities()
            {
                new ArgumentIdentity("key1", DataType.Text)
            };

            Command argsCommand = new Command(identity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key1", 36, DataType.Text)
            };

            CommandCheckerContext context = new CommandCheckerContext(identity, argsCommand);
            var result = await checker.CheckAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument value does not match the mapped type. argument=key1 type=System.String data_type=Text value_type=Int32 value=36");
        }

        [TestMethod]
        public async Task ObsoleteArgumentAndObsoleteAllowedShouldNotErrorAsync()
        {
            CommandIdentity noArgsIdentity = new("id1", "name1", "prefix1");
            noArgsIdentity.ArgumentIdentities = new ArgumentIdentities()
            {
                new ArgumentIdentity("key1", DataType.Text)
                {Obsolete = true}
            };

            Command argsCommand = new Command(noArgsIdentity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key1", "value1", DataType.Text)
            };

            CommandCheckerContext context = new CommandCheckerContext(noArgsIdentity, argsCommand);

            options.Checker.AllowObsoleteArgument = true;
            var result = await checker.CheckAsync(context);
            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        public async Task ObsoleteArgumentAndObsoleteNotAllowedShouldErrorAsync()
        {
            CommandIdentity noArgsIdentity = new CommandIdentity("id1", "name1", "prefix1");
            noArgsIdentity.ArgumentIdentities = new ArgumentIdentities()
            {
                new ArgumentIdentity("key1", DataType.Text)
                {Obsolete = true}
            };

            Command argsCommand = new Command(noArgsIdentity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key1", "value1", DataType.Text)
            };

            CommandCheckerContext context = new CommandCheckerContext(noArgsIdentity, argsCommand);

            options.Checker.AllowObsoleteArgument = null;
            var result = await checker.CheckAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument is obsolete. command_name=name1 command_id=id1 argument=key1");

            options.Checker.AllowObsoleteArgument = false;
            result = await checker.CheckAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument is obsolete. command_name=name1 command_id=id1 argument=key1");
        }

        [TestMethod]
        public async Task RequiredArgumentMissingShouldErrorAsync()
        {
            CommandIdentity identity = new CommandIdentity("id1", "name1", "prefix1");
            identity.ArgumentIdentities = new ArgumentIdentities()
            {
                new ArgumentIdentity("key1", DataType.Text, required:true)
            };

            Command argsCommand = new Command(identity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key2", "value2", DataType.Text)
            };

            CommandCheckerContext context = new CommandCheckerContext(identity, argsCommand);
            var result = await checker.CheckAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.MissingArgument, "The required argument is missing. command_name=name1 command_id=id1 argument=key1");
        }

        [TestMethod]
        public async Task RequiredArgumentPassedShouldNotErrorAsync()
        {
            CommandIdentity identity = new CommandIdentity("id1", "name1", "prefix1");
            identity.ArgumentIdentities = new ArgumentIdentities()
            {
                new ArgumentIdentity("key1", DataType.Text, required:true)
            };

            Command argsCommand = new Command(identity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key1", "value1", DataType.Text)
            };

            CommandCheckerContext context = new CommandCheckerContext(identity, argsCommand);
            var result = await checker.CheckAsync(context);
            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        public async Task ValueValidShouldNotErrorAsync()
        {
            CommandIdentity identity = new CommandIdentity("id1", "name1", "prefix1");
            identity.ArgumentIdentities = new ArgumentIdentities()
            {
                new ArgumentIdentity("key1", DataType.Text)
            };

            Command argsCommand = new Command(identity);
            argsCommand.Arguments = new Arguments()
            {
                new Argument("key1", "value1", DataType.Text)
            };

            CommandCheckerContext context = new CommandCheckerContext(identity, argsCommand);
            var result = await checker.CheckAsync(context);
            Assert.IsFalse(result.IsError);
        }

        protected override void OnTestInitialize()
        {
            options = MockCliOptions.New();
            mapper = new DataAnnotationsArgumentDataTypeMapper(options, TestLogger.Create<DataAnnotationsArgumentDataTypeMapper>());
            valueChecker = new ArgumentChecker(mapper, options, TestLogger.Create<ArgumentChecker>());
            checker = new CommandChecker(valueChecker, options, TestLogger.Create<CommandChecker>());
            commands = new InMemoryCommandIdentityStore(MockCommands.Commands, options, TestLogger.Create<InMemoryCommandIdentityStore>());
        }

        private CommandIdentity NewCustomDataTypeCmdIdentity(string customDataType)
        {
            CommandIdentity identity = new CommandIdentity("id1", "name1", "prefix1");
            identity.ArgumentIdentities = new ArgumentIdentities()
            {
                new ArgumentIdentity("key1", customDataType)
            };
            return identity;
        }

        private Command NewDataTypeCmd(CommandIdentity Command, object value)
        {
            Command cmd = new Command(Command);
            cmd.Arguments![0].Value = value;
            return cmd;
        }

        private CommandIdentity NewDataTypeCmdIdentity(DataType dataType)
        {
            CommandIdentity identity = new CommandIdentity("id1", "name1", "prefix1");
            identity.ArgumentIdentities = new ArgumentIdentities()
            {
                new ArgumentIdentity("key1", dataType)
            };
            return identity;
        }

        private CommandChecker checker = null!;
        private ICommandIdentityStore commands = null!;
        private IArgumentDataTypeMapper mapper = null!;
        private CliOptions options = null!;
        private IArgumentChecker valueChecker = null!;
    }
}
