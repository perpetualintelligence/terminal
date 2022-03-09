/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Handlers.Mocks;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Cli.Mocks;

using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers
{
    [TestClass]
    public class CommandHandlerTests : InitializerTests
    {
        public CommandHandlerTests() : base(TestLogger.Create<CommandHandlerTests>())
        {
        }

        [TestMethod]
        public async Task CheckerConfiguredButNotAddedToServiceCollectionShouldErrorAsync()
        {
            // Mock checker configured
            command.Item1._checker = typeof(MockCommandCheckerInner);

            // No mock checker added to collection.
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>());
            using var newhost = hostBuilder.Build();

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, licenses);
            var newHandler = new CommandHandler(newhost.Services, licenseChecker, options, TestLogger.Create<CommandHandler>());
            await TestHelper.AssertThrowsErrorExceptionAsync(() => newHandler.HandleAsync(commandContext), Errors.ServerError, "The command checker is not registered with service collection. command_name=name1 command_id=id1 checker=PerpetualIntelligence.Cli.Commands.Handlers.Mocks.MockCommandCheckerInner");
        }

        [TestMethod]
        public async Task CheckerErrorShouldErrorHandler()
        {
            command.Item1._checker = typeof(MockErrorCommandCheckerInner);

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, licenses);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), "test_checker_error", "test_checker_error_desc");
        }

        [TestMethod]
        public async Task CheckerNotConfiguredShouldError()
        {
            CommandHandlerContext commandContext = new(command.Item1, command.Item2, licenses);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), Errors.ServerError, "The command checker is not configured. command_name=name1 command_id=id1");
        }

        [TestMethod]
        public async Task InvalidCheckerShouldError()
        {
            // Not a ICommandChecker
            command.Item1._checker = typeof(MockNotCheckerOrRunner);

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, licenses);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), Errors.ServerError, "The command checker is not valid. command_name=name1 command_id=id1 checker=PerpetualIntelligence.Cli.Commands.Handlers.Mocks.MockNotCheckerOrRunner");
        }

        [TestMethod]
        public async Task InvalidRunnerShouldError()
        {
            // Make sure checker pass so runner can fail
            command.Item1._checker = typeof(MockCommandCheckerInner);

            // Not a ICommandChecker
            command.Item1._runner = typeof(MockNotCheckerOrRunner);

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, licenses);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), Errors.ServerError, "The command runner is not valid. command_name=name1 command_id=id1 runner=PerpetualIntelligence.Cli.Commands.Handlers.Mocks.MockNotCheckerOrRunner");
        }

        [TestMethod]
        public async Task RunnerConfiguredButNotAddedToServiceCollectionShouldErrorAsync()
        {
            // Make sure checker pass so runner can fail. // No mock runner added to collection. Configure checker so
            // checker can pass
            command.Item1._checker = typeof(MockCommandCheckerInner);
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureCheckerOnly);

            // Mock checker configured but not added to service collection
            command.Item1._runner = typeof(MockCommandRunnerInner);

            using var newhost = hostBuilder.Build();
            var newHandler = new CommandHandler(newhost.Services, licenseChecker, options, TestLogger.Create<CommandHandler>());
            CommandHandlerContext commandContext = new(command.Item1, command.Item2, licenses);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => newHandler.HandleAsync(commandContext), Errors.ServerError, "The command runner is not registered with service collection. command_name=name1 command_id=id1 runner=PerpetualIntelligence.Cli.Commands.Handlers.Mocks.MockCommandRunnerInner");
        }

        [TestMethod]
        public async Task RunnerErrorShouldErrorHandler()
        {
            // Make sure checker pass so runner can fail
            command.Item1._checker = typeof(MockCommandCheckerInner);

            command.Item1._runner = typeof(MockErrorCommandRunnerInner);

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, licenses);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), "test_runner_error", "test_runner_error_desc");
        }

        [TestMethod]
        public async Task RunnerNotConfiguredShouldError()
        {
            // Make sure checker pass so runner can fail
            command.Item1._checker = typeof(MockCommandCheckerInner);

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, licenses);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), Errors.ServerError, "The command runner is not configured. command_name=name1 command_id=id1");
        }

        [TestMethod]
        public async Task ValidCheckerAndRunnerShouldCheckCorrectLicenseFeaturesAsync()
        {
            command.Item1._checker = typeof(MockCommandCheckerInner);
            command.Item1._runner = typeof(MockCommandRunnerInner);

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, licenses);
            var result = await handler.HandleAsync(commandContext);

            Assert.IsTrue(licenseChecker.Called);
            Assert.AreEqual(LicenseCheckerFeature.RootCommandLimit | LicenseCheckerFeature.CommandGroupLimit | LicenseCheckerFeature.CommandLimit, licenseChecker.ContextCalled.CheckFeature);
            Assert.AreEqual(command.Item1, licenseChecker.ContextCalled.CommandDescriptor);
            CollectionAssert.AreEqual(licenses.ToArray(), licenseChecker.ContextCalled.Licenses.ToArray());
        }

        [TestMethod]
        public async Task ValidCheckerAndRunnerShouldlAllowHandlerAsync()
        {
            command.Item1._checker = typeof(MockCommandCheckerInner);
            command.Item1._runner = typeof(MockCommandRunnerInner);

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, licenses);
            var result = await handler.HandleAsync(commandContext);
        }

        protected override void OnTestInitialize()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServices);
            host = hostBuilder.Build();

            licenses = MockLicenses.SingleLicense;
            licenseChecker = new MockLicenseCheckerInner();
            command = MockCommands.NewCommandDefinition("id1", "name1", "prefix1", "desc1");
            options = MockCliOptions.New();
            handler = new CommandHandler(host.Services, licenseChecker, options, TestLogger.Create<CommandHandler>());
        }

        private void ConfigureCheckerOnly(IServiceCollection arg2)
        {
            arg2.AddTransient<MockCommandCheckerInner>();
        }

        private void ConfigureServices(IServiceCollection arg2)
        {
            arg2.AddTransient<MockCommandCheckerInner>();
            arg2.AddTransient<MockErrorCommandCheckerInner>();

            arg2.AddTransient<MockCommandRunnerInner>();
            arg2.AddTransient<MockErrorCommandRunnerInner>();

            arg2.AddTransient<MockNotCheckerOrRunner>();
        }

        private Tuple<CommandDescriptor, Command> command = null!;
        private CommandHandler handler = null!;
        private IHost host = null!;
        private MockLicenseCheckerInner licenseChecker = null!;
        private IEnumerable<License> licenses = null!;
        private CliOptions options = null!;
    }
}
