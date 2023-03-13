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
            ArgumentDescriptor argumentDescriptor = new("key1", DataType.Text, "desc1") { Disabled = true };
            CommandDescriptor disabledArgsDescriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Arguments arguments = new(textHandler)
            {
                new Argument(argumentDescriptor, "value1")
            };
            Command argsCommand = new(disabledArgsDescriptor, arguments);

            CommandCheckerContext context = new(disabledArgsDescriptor, argsCommand);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument is disabled. command_name=name1 command_id=id1 argument=key1");
        }

        [TestMethod]
        public async Task EnabledArgumentShouldNotErrorAsync()
        {
            ArgumentDescriptor argumentDescriptor = new ArgumentDescriptor("key1", DataType.Text, "desc1") { Disabled = false };
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument(argumentDescriptor, "value1"));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ObsoleteArgumentAndObsoleteAllowedShouldNotErrorAsync()
        {
            ArgumentDescriptor argumentDescriptor = new ArgumentDescriptor("key1", DataType.Text, "desc1") { Obsolete = true };
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument(argumentDescriptor, "value1"));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);

            options.Checker.AllowObsoleteArgument = true;
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ObsoleteArgumentAndObsoleteNotAllowedShouldErrorAsync()
        {
            ArgumentDescriptor argumentDescriptor = new ArgumentDescriptor("key1", DataType.Text, "desc1") { Obsolete = true };
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument(argumentDescriptor, "value1"));
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
            ArgumentDescriptor argumentDescriptor = new ArgumentDescriptor("key1", DataType.Text, "desc1", required: true);
            ArgumentDescriptor argumentDescriptor2 = new ArgumentDescriptor("key2", DataType.Text, "desc1", required: true);
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument(argumentDescriptor2, "value2"));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.MissingArgument, "The required argument is missing. command_name=name1 command_id=id1 argument=key1");
        }

        [TestMethod]
        public async Task RequiredArgumentPassedShouldNotErrorAsync()
        {
            ArgumentDescriptor argumentDescriptor = new ArgumentDescriptor("key1", DataType.Text, "desc1", required: true);
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument(argumentDescriptor, "value1"));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabledInvalidValueTypeShouldNotErrorAsync()
        {
            options.Checker.StrictArgumentValueType = false;

            ArgumentDescriptor argumentDescriptor = new ArgumentDescriptor("key1", DataType.Date, "desc1");
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument(argumentDescriptor, "non-date"));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            CommandCheckerResult result = await checker.CheckAsync(context);

            Assert.AreEqual("non-date", arguments[0].Value);
        }

        [TestMethod]
        public async Task StrictTypeCheckingWithInvalidValueTypeShouldErrorAsync()
        {
            options.Checker.StrictArgumentValueType = true;

            ArgumentDescriptor argumentDescriptor = new ArgumentDescriptor("key1", DataType.Date, "desc1");
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument(argumentDescriptor, "non-date"));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument value does not match the mapped type. argument=key1 type=System.DateTime data_type=Date value_type=String value=non-date");
        }

        [TestMethod]
        public async Task StrictTypeCheckingWithValidValueTypeShouldChangeTypeCorrectlyAsync()
        {
            options.Checker.StrictArgumentValueType = true;

            ArgumentDescriptor argumentDescriptor = new ArgumentDescriptor("key1", DataType.Date, "desc1");
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument(argumentDescriptor, "25-Mar-2021"));
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
            ArgumentDescriptor argumentDescriptor = new ArgumentDescriptor("key1", nameof(Double), "test desc");
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument(argumentDescriptor, 25.36));
            Command argsCommand = new(descriptor, arguments);

            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ValueValidShouldNotErrorAsync()
        {
            ArgumentDescriptor argumentDescriptor = new ArgumentDescriptor("key1", DataType.DateTime, "desc1");
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Arguments arguments = new(textHandler);
            arguments.Add(new Argument(argumentDescriptor, DateTime.Now));
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