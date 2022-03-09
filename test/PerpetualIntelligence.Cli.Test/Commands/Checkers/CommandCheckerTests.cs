/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Comparers;
using PerpetualIntelligence.Cli.Commands.Mappers;
using PerpetualIntelligence.Cli.Commands.Stores;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Cli.Stores.InMemory;
using PerpetualIntelligence.Protocols.Abstractions.Comparers;

using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    [TestClass]
    public class CommandCheckerTests : InitializerTests
    {
        public CommandCheckerTests() : base(TestLogger.Create<CommandCheckerTests>())
        {
        }

        [TestMethod]
        public async Task DisabledArgumentShouldErrorAsync()
        {
            CommandDescriptor disabledArgsDescriptor = new("id1", "name1", "prefix1", "desc1", new(stringComparer, new[] { new ArgumentDescriptor("key1", DataType.Text) { Disabled = true } }));

            Arguments arguments = new(stringComparer);
            arguments.Add(new Argument("key1", "value1", DataType.Text));
            Command argsCommand = new(disabledArgsDescriptor, arguments);

            CommandCheckerContext context = new(disabledArgsDescriptor, argsCommand);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument is disabled. command_name=name1 command_id=id1 argument=key1");
        }

        [TestMethod]
        public async Task EnabledArgumentShouldNotErrorAsync()
        {
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(stringComparer, new[] { new ArgumentDescriptor("key1", DataType.Text) { Disabled = false } }));

            Arguments arguments = new(stringComparer);
            arguments.Add(new Argument("key1", "value1", DataType.Text));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task InvalidValyeTypeShouldErrorAsync()
        {
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(stringComparer, new[] { new ArgumentDescriptor("key1", DataType.Text) }));

            Arguments arguments = new(stringComparer);
            arguments.Add(new Argument("key1", 36, DataType.Text));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument value does not match the mapped type. argument=key1 type=System.String data_type=Text value_type=Int32 value=36");
        }

        [TestMethod]
        public async Task ObsoleteArgumentAndObsoleteAllowedShouldNotErrorAsync()
        {
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(stringComparer, new[] { new ArgumentDescriptor("key1", DataType.Text) { Obsolete = true } }));

            Arguments arguments = new(stringComparer);
            arguments.Add(new Argument("key1", "value1", DataType.Text));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);

            options.Checker.AllowObsoleteArgument = true;
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ObsoleteArgumentAndObsoleteNotAllowedShouldErrorAsync()
        {
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(stringComparer, new[] { new ArgumentDescriptor("key1", DataType.Text) { Obsolete = true } }));

            Arguments arguments = new(stringComparer);
            arguments.Add(new Argument("key1", "value1", DataType.Text));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);

            options.Checker.AllowObsoleteArgument = null;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument is obsolete. command_name=name1 command_id=id1 argument=key1");

            options.Checker.AllowObsoleteArgument = false;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument is obsolete. command_name=name1 command_id=id1 argument=key1");
        }

        [TestMethod]
        public async Task RequiredArgumentMissingShouldErrorAsync()
        {
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(stringComparer, new[] { new ArgumentDescriptor("key1", DataType.Text, required: true) }));

            Arguments arguments = new(stringComparer);
            arguments.Add(new Argument("key2", "value2", DataType.Text));
            Command argsCommand = new("id1", "name1", "desc1", arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.MissingArgument, "The required argument is missing. command_name=name1 command_id=id1 argument=key1");
        }

        [TestMethod]
        public async Task RequiredArgumentPassedShouldNotErrorAsync()
        {
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(stringComparer, new[] { new ArgumentDescriptor("key1", DataType.Text, required: true) }));

            Arguments arguments = new(stringComparer);
            arguments.Add(new Argument("key1", "value1", DataType.Text));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ValueValidCustomDataTypeShouldNotErrorAsync()
        {
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(stringComparer, new[] { new ArgumentDescriptor("key1", nameof(Double)) }));

            Arguments arguments = new(stringComparer);
            arguments.Add(new Argument("key1", 25.36, nameof(Double)));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ValueValidShouldNotErrorAsync()
        {
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(stringComparer, new[] { new ArgumentDescriptor("key1", DataType.DateTime) }));

            Arguments arguments = new(stringComparer);
            arguments.Add(new Argument("key1", DateTime.Now, DataType.DateTime));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        protected override void OnTestInitialize()
        {
            options = MockCliOptions.New();
            stringComparer = new StringComparisonComparer(System.StringComparison.Ordinal);
            mapper = new DataAnnotationsArgumentDataTypeMapper(options, TestLogger.Create<DataAnnotationsArgumentDataTypeMapper>());
            valueChecker = new ArgumentChecker(mapper, options, TestLogger.Create<ArgumentChecker>());
            checker = new CommandChecker(valueChecker, options, TestLogger.Create<CommandChecker>());
            commands = new InMemoryCommandDescriptorStore(stringComparer, MockCommands.Commands, options, TestLogger.Create<InMemoryCommandDescriptorStore>());
        }

        private CommandChecker checker = null!;
        private ICommandDescriptorStore commands = null!;
        private IArgumentDataTypeMapper mapper = null!;
        private CliOptions options = null!;
        private IStringComparer stringComparer = null!;
        private IArgumentChecker valueChecker = null!;
    }
}
