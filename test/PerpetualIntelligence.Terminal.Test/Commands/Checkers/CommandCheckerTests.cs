/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Mappers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Runtime;
using PerpetualIntelligence.Terminal.Stores;
using PerpetualIntelligence.Terminal.Stores.InMemory;

using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
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
            OptionDescriptor optionDescriptor = new("key1", DataType.Text, "desc1") { Disabled = true };
            CommandDescriptor disabledArgsDescriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler)
            {
                new Option(optionDescriptor, "value1")
            };
            Command argsCommand = new(commandRoute, disabledArgsDescriptor, options);

            CommandHandlerContext handlerContext = new(routerContext, argsCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), TerminalErrors.InvalidOption, "The option is disabled. command_name=name1 command_id=id1 option=key1");
        }

        [TestMethod]
        public async Task EnabledArgumentShouldNotErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", DataType.Text, "desc1") { Disabled = false };
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler)
            {
                new Option(optionDescriptor, "value1")
            };
            Command argsCommand = new(commandRoute, descriptor, options);

            CommandHandlerContext handlerContext = new(routerContext, argsCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ObsoleteArgumentAndObsoleteAllowedShouldNotErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", DataType.Text, "desc1") { Obsolete = true };
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler)
            {
                new Option(optionDescriptor, "value1")
            };
            Command argsCommand = new(commandRoute, descriptor, options);

            CommandHandlerContext handlerContext = new(routerContext, argsCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);

            terminalOptions.Checker.AllowObsoleteOption = true;
            await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ObsoleteArgumentAndObsoleteNotAllowedShouldErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", DataType.Text, "desc1") { Obsolete = true };
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler)
            {
                new Option(optionDescriptor, "value1")
            };
            Command argsCommand = new(commandRoute, descriptor, options);

            CommandHandlerContext handlerContext = new(routerContext, argsCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);

            terminalOptions.Checker.AllowObsoleteOption = null;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), TerminalErrors.InvalidOption, "The option is obsolete. command_name=name1 command_id=id1 option=key1");

            terminalOptions.Checker.AllowObsoleteOption = false;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), TerminalErrors.InvalidOption, "The option is obsolete. command_name=name1 command_id=id1 option=key1");
        }

        [TestMethod]
        public async Task RequiredArgumentMissingShouldErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", DataType.Text, "desc1", required: true);
            OptionDescriptor optionDescriptor2 = new("key2", DataType.Text, "desc1", required: true);
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler)
            {
                new Option(optionDescriptor2, "value2")
            };
            Command argsCommand = new(commandRoute, descriptor, options);

            CommandHandlerContext handlerContext = new(routerContext, argsCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), TerminalErrors.MissingOption, "The required option is missing. command_name=name1 command_id=id1 option=key1");
        }

        [TestMethod]
        public async Task RequiredArgumentPassedShouldNotErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", DataType.Text, "desc1", required: true);
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler)
            {
                new Option(optionDescriptor, "value1")
            };
            Command argsCommand = new(commandRoute, descriptor, options);

            CommandHandlerContext handlerContext = new(routerContext, argsCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabledInvalidValueTypeShouldNotErrorAsync()
        {
            terminalOptions.Checker.StrictOptionValueType = false;

            OptionDescriptor optionDescriptor = new("key1", DataType.Date, "desc1");
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler)
            {
                new Option(optionDescriptor, "non-date")
            };
            Command argsCommand = new(commandRoute, descriptor, options);

            CommandHandlerContext handlerContext = new(routerContext, argsCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await checker.CheckAsync(context);

            Assert.AreEqual("non-date", options[0].Value);
        }

        [TestMethod]
        public async Task StrictTypeCheckingWithInvalidValueTypeShouldErrorAsync()
        {
            terminalOptions.Checker.StrictOptionValueType = true;

            OptionDescriptor optionDescriptor = new("key1", DataType.Date, "desc1");
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler)
            {
                new Option(optionDescriptor, "non-date")
            };
            Command argsCommand = new(commandRoute, descriptor, options);

            CommandHandlerContext handlerContext = new(routerContext, argsCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), TerminalErrors.InvalidOption, "The option value does not match the mapped type. option=key1 type=System.DateTime data_type=Date value_type=String value=non-date");
        }

        [TestMethod]
        public async Task StrictTypeCheckingWithValidValueTypeShouldChangeTypeCorrectlyAsync()
        {
            terminalOptions.Checker.StrictOptionValueType = true;

            OptionDescriptor optionDescriptor = new("key1", DataType.Date, "desc1");
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler)
            {
                new Option(optionDescriptor, "25-Mar-2021")
            };
            Command argsCommand = new(commandRoute, descriptor, options);

            object oldValue = options[0].Value;
            Assert.IsInstanceOfType(oldValue, typeof(string));

            // Value should pass and converted to date
            CommandHandlerContext handlerContext = new(routerContext, argsCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await checker.CheckAsync(context);

            object newValue = options[0].Value;
            Assert.IsInstanceOfType(newValue, typeof(DateTime));
            Assert.AreEqual(oldValue, ((DateTime)newValue).ToString("dd-MMM-yyyy"));
        }

        [TestMethod]
        public async Task ValueValidCustomDataTypeShouldNotErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(Double), "test desc");
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler)
            {
                new Option(optionDescriptor, 25.36)
            };
            Command argsCommand = new(commandRoute, descriptor, options);

            CommandHandlerContext handlerContext = new(routerContext, argsCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            var result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task ValueValidShouldNotErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", DataType.DateTime, "desc1");
            CommandDescriptor descriptor = new("id1", "name1", "prefix1", "desc1", new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler)
            {
                new Option(optionDescriptor, DateTime.Now)
            };
            Command argsCommand = new(commandRoute, descriptor, options);

            CommandHandlerContext handlerContext = new(routerContext, argsCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await checker.CheckAsync(context);
        }

        protected override void OnTestInitialize()
        {
            commandRoute = new CommandRoute(Guid.NewGuid().ToString(), "test_raw");
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            textHandler = new UnicodeTextHandler();
            mapper = new DataAnnotationsOptionDataTypeMapper(terminalOptions, TestLogger.Create<DataAnnotationsOptionDataTypeMapper>());
            valueChecker = new OptionChecker(mapper, terminalOptions);
            checker = new CommandChecker(valueChecker, terminalOptions, TestLogger.Create<CommandChecker>());
            commands = new InMemoryCommandStore(textHandler, MockCommands.Commands, terminalOptions, TestLogger.Create<InMemoryCommandStore>());
            tokenSource = new CancellationTokenSource();
            routingContext = new MockTerminalRoutingContext(new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Custom), tokenSource.Token));
            routerContext = new CommandRouterContext("test", routingContext);
        }

        private CommandRoute commandRoute = null!;
        private CommandChecker checker = null!;
        private ICommandStoreHandler commands = null!;
        private IOptionDataTypeMapper mapper = null!;
        private TerminalOptions terminalOptions = null!;
        private ITextHandler textHandler = null!;
        private IOptionChecker valueChecker = null!;
        private CommandRouterContext routerContext = null!;
        private TerminalRoutingContext routingContext = null!;
        private CancellationTokenSource tokenSource = null!;
        private readonly CommandHandlerContext handlerContext = null!;
    }
}