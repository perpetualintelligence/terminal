/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
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
            OptionDescriptor argumentDescriptor = new("key1", DataType.Text, "desc1") { Disabled = true };
            CommandDescriptor disabledArgsDescriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Options options = new(textHandler)
            {
                new Option(argumentDescriptor, "value1")
            };
            Command argsCommand = new(disabledArgsDescriptor, options);

            CommandCheckerContext context = new(disabledArgsDescriptor, argsCommand);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidOption, "The option is disabled. command_name=name1 command_id=id1 option=key1");
        }

        [TestMethod]
        public async Task EnabledArgumentShouldNotErrorAsync()
        {
            OptionDescriptor argumentDescriptor = new OptionDescriptor("key1", DataType.Text, "desc1") { Disabled = false };
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Options options = new(textHandler);
            options.Add(new Option(argumentDescriptor, "value1"));
            Command argsCommand = new(descriptor, options);

            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ObsoleteArgumentAndObsoleteAllowedShouldNotErrorAsync()
        {
            OptionDescriptor argumentDescriptor = new OptionDescriptor("key1", DataType.Text, "desc1") { Obsolete = true };
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Options options = new(textHandler);
            options.Add(new Option(argumentDescriptor, "value1"));
            Command argsCommand = new(descriptor, options);

            CommandCheckerContext context = new(descriptor, argsCommand);

            cliOptions.Checker.AllowObsoleteOption = true;
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ObsoleteArgumentAndObsoleteNotAllowedShouldErrorAsync()
        {
            OptionDescriptor argumentDescriptor = new OptionDescriptor("key1", DataType.Text, "desc1") { Obsolete = true };
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Options options = new(textHandler);
            options.Add(new Option(argumentDescriptor, "value1"));
            Command argsCommand = new(descriptor, options);

            CommandCheckerContext context = new(descriptor, argsCommand);

            cliOptions.Checker.AllowObsoleteOption = null;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidOption, "The option is obsolete. command_name=name1 command_id=id1 option=key1");

            cliOptions.Checker.AllowObsoleteOption = false;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidOption, "The option is obsolete. command_name=name1 command_id=id1 option=key1");
        }

        [TestMethod]
        public async Task RequiredArgumentMissingShouldErrorAsync()
        {
            OptionDescriptor argumentDescriptor = new OptionDescriptor("key1", DataType.Text, "desc1", required: true);
            OptionDescriptor argumentDescriptor2 = new OptionDescriptor("key2", DataType.Text, "desc1", required: true);
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Options options = new(textHandler);
            options.Add(new Option(argumentDescriptor2, "value2"));
            Command argsCommand = new(descriptor, options);

            CommandCheckerContext context = new(descriptor, argsCommand);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.MissingOption, "The required option is missing. command_name=name1 command_id=id1 option=key1");
        }

        [TestMethod]
        public async Task RequiredArgumentPassedShouldNotErrorAsync()
        {
            OptionDescriptor argumentDescriptor = new OptionDescriptor("key1", DataType.Text, "desc1", required: true);
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Options options = new(textHandler);
            options.Add(new Option(argumentDescriptor, "value1"));
            Command argsCommand = new(descriptor, options);

            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabledInvalidValueTypeShouldNotErrorAsync()
        {
            cliOptions.Checker.StrictOptionValueType = false;

            OptionDescriptor argumentDescriptor = new OptionDescriptor("key1", DataType.Date, "desc1");
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Options options = new(textHandler);
            options.Add(new Option(argumentDescriptor, "non-date"));
            Command argsCommand = new(descriptor, options);

            CommandCheckerContext context = new(descriptor, argsCommand);
            CommandCheckerResult result = await checker.CheckAsync(context);

            Assert.AreEqual("non-date", options[0].Value);
        }

        [TestMethod]
        public async Task StrictTypeCheckingWithInvalidValueTypeShouldErrorAsync()
        {
            cliOptions.Checker.StrictOptionValueType = true;

            OptionDescriptor argumentDescriptor = new OptionDescriptor("key1", DataType.Date, "desc1");
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Options options = new(textHandler);
            options.Add(new Option(argumentDescriptor, "non-date"));
            Command argsCommand = new(descriptor, options);

            CommandCheckerContext context = new(descriptor, argsCommand);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidOption, "The option value does not match the mapped type. option=key1 type=System.DateTime data_type=Date value_type=String value=non-date");
        }

        [TestMethod]
        public async Task StrictTypeCheckingWithValidValueTypeShouldChangeTypeCorrectlyAsync()
        {
            cliOptions.Checker.StrictOptionValueType = true;

            OptionDescriptor argumentDescriptor = new OptionDescriptor("key1", DataType.Date, "desc1");
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Options options = new(textHandler);
            options.Add(new Option(argumentDescriptor, "25-Mar-2021"));
            Command argsCommand = new(descriptor, options);

            object oldValue = options[0].Value;
            Assert.IsInstanceOfType(oldValue, typeof(string));

            // Value should pass and converted to date
            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);

            object newValue = options[0].Value;
            Assert.IsInstanceOfType(newValue, typeof(DateTime));
            Assert.AreEqual(oldValue, ((DateTime)newValue).ToString("dd-MMM-yyyy"));
        }

        [TestMethod]
        public async Task ValueValidCustomDataTypeShouldNotErrorAsync()
        {
            OptionDescriptor argumentDescriptor = new OptionDescriptor("key1", nameof(Double), "test desc");
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Options options = new(textHandler);
            options.Add(new Option(argumentDescriptor, 25.36));
            Command argsCommand = new(descriptor, options);

            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ValueValidShouldNotErrorAsync()
        {
            OptionDescriptor argumentDescriptor = new OptionDescriptor("key1", DataType.DateTime, "desc1");
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { argumentDescriptor }));

            Options options = new(textHandler);
            options.Add(new Option(argumentDescriptor, DateTime.Now));
            Command argsCommand = new(descriptor, options);

            CommandCheckerContext context = new(descriptor, argsCommand);
            var result = await checker.CheckAsync(context);
        }

        protected override void OnTestInitialize()
        {
            cliOptions = MockCliOptions.New();
            textHandler = new UnicodeTextHandler();
            mapper = new DataAnnotationsOptionDataTypeMapper(cliOptions, TestLogger.Create<DataAnnotationsOptionDataTypeMapper>());
            valueChecker = new OptionChecker(mapper, cliOptions, TestLogger.Create<OptionChecker>());
            checker = new CommandChecker(valueChecker, cliOptions, TestLogger.Create<CommandChecker>());
            commands = new InMemoryCommandStore(textHandler, MockCommands.Commands, cliOptions, TestLogger.Create<InMemoryCommandStore>());
        }

        private CommandChecker checker = null!;
        private ICommandStoreHandler commands = null!;
        private IOptionDataTypeMapper mapper = null!;
        private CliOptions cliOptions = null!;
        private ITextHandler textHandler = null!;
        private IOptionChecker valueChecker = null!;
    }
}