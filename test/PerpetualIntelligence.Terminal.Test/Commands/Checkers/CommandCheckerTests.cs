/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Commands.Parsers;
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
using System.Collections.Generic;
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
        public async Task Router_Throws_On_CommandString_MaxLimitAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(String), "desc1", OptionFlags.Disabled);
            CommandDescriptor disabledArgsDescriptor = new("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None, optionDescriptors: new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler, new Option[] { new Option(optionDescriptor, "value1") });

            Command argsCommand = new(disabledArgsDescriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(routerContext.Route, argsCommand, Root.Default());

            // Max 30 we are passing 31
            terminalOptions.Router.MaxMessageLength = 30;
            routerContext = new CommandRouterContext(new string('x', 31), routingContext);
            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);

            Func<Task> act = () => checker.CheckCommandAsync(context);
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The command string is too long. command=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx max=30");
        }

        [TestMethod]
        public async Task DisabledOptionShouldErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(String), "desc1", OptionFlags.Disabled);
            CommandDescriptor disabledArgsDescriptor = new("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None, optionDescriptors: new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler, new Option[] { new Option(optionDescriptor, "value1") });

            Command argsCommand = new(disabledArgsDescriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(routerContext.Route, argsCommand, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => checker.CheckCommandAsync(context), TerminalErrors.InvalidOption, "The option is disabled. command=id1 option=key1");
        }

        [TestMethod]
        public async Task EnabledOptionShouldNotErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(String), "desc1", OptionFlags.None);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None, optionDescriptors: new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler, new Option[] { new Option(optionDescriptor, "value1") });

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(routerContext.Route, argsCommand, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await checker.CheckCommandAsync(context);
        }

        [TestMethod]
        public async Task ObsoleteOptionAndObsoleteAllowedShouldNotErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(String), "desc1", OptionFlags.Obsolete);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None, optionDescriptors: new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler, new Option[] { new Option(optionDescriptor, "value1") });

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(routerContext.Route, argsCommand, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);

            terminalOptions.Checker.AllowObsolete = true;
            await checker.CheckCommandAsync(context);
        }

        [TestMethod]
        public async Task ObsoleteOptionAndObsoleteNotAllowedShouldErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(String), "desc1", OptionFlags.Obsolete);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None, optionDescriptors: new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler, new Option[] { new Option(optionDescriptor, "value1") });

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(routerContext.Route, argsCommand, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);

            terminalOptions.Checker.AllowObsolete = null;
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => checker.CheckCommandAsync(context), TerminalErrors.InvalidOption, "The option is obsolete. command=id1 option=key1");

            terminalOptions.Checker.AllowObsolete = false;
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => checker.CheckCommandAsync(context), TerminalErrors.InvalidOption, "The option is obsolete. command=id1 option=key1");
        }

        [TestMethod]
        public async Task RequiredOptionMissingShouldErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(String), "desc1", OptionFlags.Required);
            OptionDescriptor optionDescriptor2 = new("key2", nameof(String), "desc1", OptionFlags.Required);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None, optionDescriptors: new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler, new Option[] { new Option(optionDescriptor2, "value2") });

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(routerContext.Route, argsCommand, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => checker.CheckCommandAsync(context), TerminalErrors.MissingOption, "The required option is missing. command=id1 option=key1");
        }

        [TestMethod]
        public async Task RequiredOptionPassedShouldNotErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(String), "desc1", OptionFlags.Required);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None, optionDescriptors: new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler, new Option[] { new Option(optionDescriptor, "value1") });

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(routerContext.Route, argsCommand, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await checker.CheckCommandAsync(context);
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabledInvalidValueTypeShouldNotErrorAsync()
        {
            terminalOptions.Checker.StrictValueType = false;

            OptionDescriptor optionDescriptor = new("key1", nameof(DateTime), "desc1", OptionFlags.None);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None, optionDescriptors: new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler, new List<Option>() { new Option(optionDescriptor, "non-date") });

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(routerContext.Route, argsCommand, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await checker.CheckCommandAsync(context);

            Assert.AreEqual("non-date", options["key1"].Value);
        }

        [TestMethod]
        public async Task StrictTypeCheckingValueDelimiterValidValueTypeShouldErrorAsync()
        {
            terminalOptions.Checker.StrictValueType = true;

            OptionDescriptor optionDescriptor = new("key1", nameof(DateTime), "desc1", OptionFlags.None);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None, optionDescriptors: new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler, new List<Option>() { new Option(optionDescriptor, "non-date") });

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(routerContext.Route, argsCommand, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => checker.CheckCommandAsync(context), TerminalErrors.InvalidOption, "The option value does not match the mapped type. option=key1 type=System.DateTime data_type=DateTime value_type=String value=non-date");
        }

        [TestMethod]
        public async Task StrictTypeCheckingWithValidValueTypeShouldChangeTypeCorrectlyAsync()
        {
            terminalOptions.Checker.StrictValueType = true;

            OptionDescriptor optionDescriptor = new("key1", nameof(DateTime), "desc1", OptionFlags.None);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None, optionDescriptors: new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler, new List<Option>() { new Option(optionDescriptor, "25-Mar-2021") });

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(routerContext.Route, argsCommand, Root.Default());

            object oldValue = options["key1"].Value;
            Assert.IsInstanceOfType(oldValue, typeof(string));

            // Value should pass and converted to date
            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await checker.CheckCommandAsync(context);

            object newValue = options["key1"].Value;
            Assert.IsInstanceOfType(newValue, typeof(DateTime));
            Assert.AreEqual(oldValue, ((DateTime)newValue).ToString("dd-MMM-yyyy"));
        }

        [TestMethod]
        public async Task ValueValidCustomDataTypeShouldNotErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(Double), "test desc", OptionFlags.None);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None, optionDescriptors: new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler, new List<Option>() { new Option(optionDescriptor, 25.36) });

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(routerContext.Route, argsCommand, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            var result = await checker.CheckCommandAsync(context);
        }

        [TestMethod]
        public async Task ValueValidShouldNotErrorAsync()
        {
            OptionDescriptor optionDescriptor = new("key1", nameof(DateTime), "desc1", OptionFlags.None);
            CommandDescriptor descriptor = new("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None, optionDescriptors: new(textHandler, new[] { optionDescriptor }));

            Options options = new(textHandler, new List<Option>() { new Option(optionDescriptor, DateTime.Now) });

            Command argsCommand = new(descriptor, arguments: null, options);
            ParsedCommand extractedCommand = new(routerContext.Route, argsCommand, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            CommandCheckerContext context = new(handlerContext);
            await checker.CheckCommandAsync(context);
        }

        protected override void OnTestInitialize()
        {
            commandRoute = new CommandRoute(Guid.NewGuid().ToString(), "test_raw");
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            textHandler = new UnicodeTextHandler();
            optionMapper = new DataTypeMapper<Option>(terminalOptions, TestLogger.Create<DataTypeMapper<Option>>());
            argumentMapper = new DataTypeMapper<Argument>(terminalOptions, TestLogger.Create<DataTypeMapper<Argument>>());
            valueChecker = new OptionChecker(optionMapper, terminalOptions);
            argumentChecker = new ArgumentChecker(argumentMapper, terminalOptions);
            checker = new CommandChecker(valueChecker, argumentChecker, terminalOptions, TestLogger.Create<CommandChecker>());
            commands = new InMemoryCommandStore(textHandler, MockCommands.Commands.Values);
            tokenSource = new CancellationTokenSource();
            routingContext = new MockTerminalRoutingContext(new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Custom), tokenSource.Token));
            routerContext = new CommandRouterContext("test", routingContext);
        }

        private CommandRoute commandRoute = null!;
        private CommandChecker checker = null!;
        private ICommandStore commands = null!;
        private IDataTypeMapper<Option> optionMapper = null!;
        private IDataTypeMapper<Argument> argumentMapper = null!;
        private TerminalOptions terminalOptions = null!;
        private ITextHandler textHandler = null!;
        private IOptionChecker valueChecker = null!;
        private IArgumentChecker argumentChecker = null!;
        private CommandRouterContext routerContext = null!;
        private TerminalRoutingContext routingContext = null!;
        private CancellationTokenSource tokenSource = null!;
        private readonly CommandHandlerContext handlerContext = null!;
    }
}