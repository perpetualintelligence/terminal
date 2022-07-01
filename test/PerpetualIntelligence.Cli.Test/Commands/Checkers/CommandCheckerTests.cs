/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Mappers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Cli.Stores;
using PerpetualIntelligence.Cli.Stores.InMemory;

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
            CommandDescriptor disabledArgsDescriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { new ArgumentDescriptor("key1", DataType.Text, "desc1") { Disabled = true } }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument("key1", "value1", DataType.Text));
            Command argsCommand = new(disabledArgsDescriptor, arguments);

            CommandCheckerContext context = new(disabledArgsDescriptor, argsCommand);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument is disabled. command_name=name1 command_id=id1 argument=key1");
        }

        [TestMethod]
        public async Task EnabledArgumentShouldNotErrorAsync()
        {
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { new ArgumentDescriptor("key1", DataType.Text, "desc1") { Disabled = false } }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument("key1", "value1", DataType.Text));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ObsoleteArgumentAndObsoleteAllowedShouldNotErrorAsync()
        {
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { new ArgumentDescriptor("key1", DataType.Text, "desc1") { Obsolete = true } }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument("key1", "value1", DataType.Text));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);

            options.Checker.AllowObsoleteArgument = true;
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ObsoleteArgumentAndObsoleteNotAllowedShouldErrorAsync()
        {
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { new ArgumentDescriptor("key1", DataType.Text, "desc1") { Obsolete = true } }));

            Arguments arguments = new(textHandler);
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
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { new ArgumentDescriptor("key1", DataType.Text, "desc1", required: true) }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument("key2", "value2", DataType.Text));
            Command argsCommand = new("id1", "name1", "desc1", arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.MissingArgument, "The required argument is missing. command_name=name1 command_id=id1 argument=key1");
        }

        [TestMethod]
        public async Task RequiredArgumentPassedShouldNotErrorAsync()
        {
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { new ArgumentDescriptor("key1", DataType.Text, "desc1", required: true) }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument("key1", "value1", DataType.Text));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabledInvalidValueTypeShouldNotErrorAsync()
        {
            options.Checker.StrictArgumentValueType = false;

            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { new ArgumentDescriptor("key1", DataType.Date, "desc1") }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument("key1", "non-date", DataType.Date));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            CommandCheckerResult result = await checker.CheckAsync(context);

            Assert.AreEqual("non-date", arguments[0].Value);
        }

        [TestMethod]
        public async Task StrictTypeCheckingWithInvalidValueTypeShouldErrorAsync()
        {
            options.Checker.StrictArgumentValueType = true;

            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { new ArgumentDescriptor("key1", DataType.Date, "desc1") }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument("key1", "non-date", DataType.Date));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument value does not match the mapped type. argument=key1 type=System.DateTime data_type=Date value_type=String value=non-date");
        }

        [TestMethod]
        public async Task StrictTypeCheckingWithValidValueTypeShouldChangeTypeCorrectlyAsync()
        {
            options.Checker.StrictArgumentValueType = true;

            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { new ArgumentDescriptor("key1", DataType.Date, "desc1") }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument("key1", "25-Mar-2021", DataType.Date));
            Command argsCommand = new(descriptor, arguments);

            object oldValue = arguments[0].Value;
            Assert.IsInstanceOfType(oldValue, typeof(string));

            // Value should pass and converted to date
            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);

            object newValue = arguments[0].Value;
            Assert.IsInstanceOfType(newValue, typeof(DateTime));
            Assert.AreEqual(oldValue, ((DateTime)newValue).ToString("dd-MMM-yyyy"));
        }

        [TestMethod]
        public async Task ValueValidCustomDataTypeShouldNotErrorAsync()
        {
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { new ArgumentDescriptor("key1", nameof(Double), "test desc") }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument("key1", 25.36, nameof(Double)));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ValueValidShouldNotErrorAsync()
        {
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { new ArgumentDescriptor("key1", DataType.DateTime, "desc1") }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument("key1", DateTime.Now, DataType.DateTime));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        protected override void OnTestInitialize()
        {
            options = MockCliOptions.New();
            textHandler = new UnicodeTextHandler();
            mapper = new DataAnnotationsArgumentDataTypeMapper(options, TestLogger.Create<DataAnnotationsArgumentDataTypeMapper>());
            valueChecker = new ArgumentChecker(mapper, options, TestLogger.Create<ArgumentChecker>());
            checker = new CommandChecker(valueChecker, options, TestLogger.Create<CommandChecker>());
            commands = new InMemoryCommandStore(textHandler, MockCommands.Commands, options, TestLogger.Create<InMemoryCommandStore>());
        }

        private CommandChecker checker = null!;
        private ICommandStoreHandler commands = null!;
        private IArgumentDataTypeMapper mapper = null!;
        private CliOptions options = null!;
        private ITextHandler textHandler = null!;
        private IArgumentChecker valueChecker = null!;
    }
}
