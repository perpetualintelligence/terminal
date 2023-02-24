/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Handlers.Mocks;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Cli.Mocks;

using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
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
            command.Item1.Checker = typeof(MockCommandCheckerInner);

            // No mock checker added to collection.
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>());
            using var newhost = hostBuilder.Build();

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, license);
            var newHandler = new CommandHandler(newhost.Services, licenseChecker, options, TestLogger.Create<CommandHandler>());
            await TestHelper.AssertThrowsErrorExceptionAsync(() => newHandler.HandleAsync(commandContext), Errors.ServerError, "The command checker is not registered with service collection. command_name=name1 command_id=id1 checker=PerpetualIntelligence.Cli.Commands.Handlers.Mocks.MockCommandCheckerInner");
        }

        [TestMethod]
        public async Task CheckerErrorShouldErrorHandler()
        {
            command.Item1.Checker = typeof(MockErrorCommandCheckerInner);

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, license);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), "test_checker_error", "test_checker_error_desc");
        }

        [TestMethod]
        public async Task CheckerNotConfiguredShouldError()
        {
            CommandHandlerContext commandContext = new(command.Item1, command.Item2, license);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), Errors.ServerError, "The command checker is not configured. command_name=name1 command_id=id1");
        }

        [TestMethod]
        public async Task HandlerShouldProcessTheResultCorrectlyAsync()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            // MockCommandRunnerInnerResult.ResultProcessed and MockCommandRunnerInnerResult.ResultDisposed are static
            // because result is not part of DI, its instance is created by the framework.
            try
            {
                Assert.IsFalse(MockCommandRunnerInnerResult.ResultProcessed);
                Assert.IsFalse(MockCommandRunnerInnerResult.ResultDisposed);

                CommandHandlerContext commandContext = new(command.Item1, command.Item2, license);
                var result = await handler.HandleAsync(commandContext);

                Assert.IsTrue(MockCommandRunnerInnerResult.ResultProcessed);
                Assert.IsFalse(MockCommandRunnerInnerResult.ResultDisposed);
            }
            finally
            {
                MockCommandRunnerInnerResult.ResultProcessed = false;
                MockCommandRunnerInnerResult.ResultDisposed = false;
            }
        }

        [TestMethod]
        public async Task InvalidCheckerShouldError()
        {
            // Not a ICommandChecker
            command.Item1.Checker = typeof(MockNotCheckerOrRunner);

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, license);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), Errors.ServerError, "The command checker is not valid. command_name=name1 command_id=id1 checker=PerpetualIntelligence.Cli.Commands.Handlers.Mocks.MockNotCheckerOrRunner");
        }

        [TestMethod]
        public async Task InvalidRunnerShouldError()
        {
            // Make sure checker pass so runner can fail
            command.Item1.Checker = typeof(MockCommandCheckerInner);

            // Not a ICommandChecker
            command.Item1.Runner = typeof(MockNotCheckerOrRunner);

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, license);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), Errors.ServerError, "The command runner delegate is not configured. command_name=name1 command_id=id1 runner=PerpetualIntelligence.Cli.Commands.Handlers.Mocks.MockNotCheckerOrRunner");
        }

        [TestMethod]
        public async Task RunnerConfiguredButNotAddedToServiceCollectionShouldErrorAsync()
        {
            // Make sure checker pass so runner can fail. // No mock runner added to collection. Configure checker so
            // checker can pass
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureCheckerOnly);

            // Mock checker configured but not added to service collection
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            using var newhost = hostBuilder.Build();
            var newHandler = new CommandHandler(newhost.Services, licenseChecker, options, TestLogger.Create<CommandHandler>());
            CommandHandlerContext commandContext = new(command.Item1, command.Item2, license);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => newHandler.HandleAsync(commandContext), Errors.ServerError, "The command runner is not registered with service collection. command_name=name1 command_id=id1 runner=PerpetualIntelligence.Cli.Commands.Handlers.Mocks.MockCommandRunnerInner");
        }

        [TestMethod]
        public async Task RunnerErrorShouldErrorHandler()
        {
            // Make sure checker pass so runner can fail
            command.Item1.Checker = typeof(MockCommandCheckerInner);

            command.Item1.Runner = typeof(MockErrorCommandRunnerInner);

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, license);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), "test_runner_error", "test_runner_error_desc");
        }

        [TestMethod]
        public async Task RunnerNotConfiguredShouldError()
        {
            // Make sure checker pass so runner can fail
            command.Item1.Checker = typeof(MockCommandCheckerInner);

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, license);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), Errors.ServerError, "The command runner is not configured. command_name=name1 command_id=id1");
        }

        [TestMethod]
        public async Task ValidCheckerAndRunnerShouldCheckCorrectLicenseFeaturesAsync()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, license);
            var result = await handler.HandleAsync(commandContext);

            Assert.IsTrue(licenseChecker.Called);
            Assert.IsNotNull(licenseChecker.ContextCalled);
            Assert.AreEqual(license, licenseChecker.ContextCalled.License);
        }

        [TestMethod]
        public async Task ValidCheckerAndRunnerShouldlAllowHandlerAsync()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, license);
            var result = await handler.HandleAsync(commandContext);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<CommandHandlerResult>(result);

            Assert.IsNotNull(result.RunnerResult);
            Assert.IsInstanceOfType<CommandRunnerResult>(result.RunnerResult);
            
        }

        [TestMethod]
        public async Task ValidGenericRunnerShouldlAllowHandlerAsync()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockGenericCommandRunnerInner);

            CommandHandlerContext commandContext = new(command.Item1, command.Item2, license);
            var result = await handler.HandleAsync(commandContext);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<CommandHandlerResult>(result);

            Assert.IsNotNull(result.RunnerResult);
            Assert.IsInstanceOfType<MockGenericCommandRunnerResult>(result.RunnerResult);
        }

        protected override void OnTestInitialize()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServices);
            host = hostBuilder.Build();

            license = MockLicenses.TestLicense;
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
            arg2.AddTransient<MockGenericCommandRunnerInner>();

            arg2.AddTransient<MockNotCheckerOrRunner>();
        }

        private Tuple<CommandDescriptor, Command> command = null!;
        private CommandHandler handler = null!;
        private IHost host = null!;
        private License license = null!;
        private MockLicenseCheckerInner licenseChecker = null!;
        private CliOptions options = null!;
    }
}