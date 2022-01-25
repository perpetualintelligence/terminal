/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Handlers.Mocks;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers
{
    [TestClass]
    public class CommandHandlerTests : LogTest
    {
        public CommandHandlerTests() : base(TestLogger.Create<CommandHandlerTests>())
        {
        }

        [TestMethod]
        public async Task CheckerConfiguredButNotAddedToServiceCollectionShouldErrorAsync()
        {
            // Mock checker configured
            command.Item1.Checker = typeof(MockChecker);

            // No mock checker added to collection.
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>());
            using var newhost = hostBuilder.Build();

            CommandHandlerContext commandHandler = new CommandHandlerContext(command.Item1, command.Item2);
            var newHandler = new CommandHandler(newhost.Services, options, TestLogger.Create<CommandHandler>());
            var result = await newHandler.HandleAsync(commandHandler);
            TestHelper.AssertOneImlxError(result, Errors.ServerError, "The command checker is not registered with service collection. command_name=name1 command_id=id1 checker=PerpetualIntelligence.Cli.Commands.Handlers.Mocks.MockChecker");
        }

        [TestMethod]
        public async Task CheckerErrorShouldFailHandler()
        {
            command.Item1.Checker = typeof(MockErrorChecker);

            CommandHandlerContext commandHandler = new CommandHandlerContext(command.Item1, command.Item2);
            var result = await handler.HandleAsync(commandHandler);

            TestHelper.AssertOneImlxError(result, "test_checker_error", "test_checker_error_desc");
        }

        [TestMethod]
        public async Task CheckerNotConfiguredShouldError()
        {
            CommandHandlerContext commandHandler = new CommandHandlerContext(command.Item1, command.Item2);
            var result = await handler.HandleAsync(commandHandler);
            TestHelper.AssertOneImlxError(result, Errors.ServerError, "The command checker is not configured. command_name=name1 command_id=id1");
        }

        [TestMethod]
        public async Task InvalidCheckerShouldError()
        {
            // Not a ICommandChecker
            command.Item1.Checker = typeof(MockNotCheckerOrRunner);

            CommandHandlerContext commandHandler = new CommandHandlerContext(command.Item1, command.Item2);
            var result = await handler.HandleAsync(commandHandler);
            TestHelper.AssertOneImlxError(result, Errors.ServerError, "The command checker is not valid. command_name=name1 command_id=id1 checker=PerpetualIntelligence.Cli.Commands.Handlers.Mocks.MockNotCheckerOrRunner");
        }

        [TestMethod]
        public async Task InvalidRunnerShouldError()
        {
            // Make sure checker pass so runner can fail
            command.Item1.Checker = typeof(MockChecker);

            // Not a ICommandChecker
            command.Item1.Runner = typeof(MockNotCheckerOrRunner);

            CommandHandlerContext commandHandler = new CommandHandlerContext(command.Item1, command.Item2);
            var result = await handler.HandleAsync(commandHandler);
            TestHelper.AssertOneImlxError(result, Errors.ServerError, "The command runner is not valid. command_name=name1 command_id=id1 runner=PerpetualIntelligence.Cli.Commands.Handlers.Mocks.MockNotCheckerOrRunner");
        }

        [TestMethod]
        public async Task RunnerConfiguredButNotAddedToServiceCollectionShouldErrorAsync()
        {
            // Make sure checker pass so runner can fail. // No mock runner added to collection. Configure checker so
            // checker can pass
            command.Item1.Checker = typeof(MockChecker);
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureCheckerOnly);

            // Mock checker configured but not added to service collection
            command.Item1.Runner = typeof(MockRunner);

            using var newhost = hostBuilder.Build();
            var newHandler = new CommandHandler(newhost.Services, options, TestLogger.Create<CommandHandler>());
            CommandHandlerContext commandHandler = new CommandHandlerContext(command.Item1, command.Item2);
            var result = await newHandler.HandleAsync(commandHandler);

            TestHelper.AssertOneImlxError(result, Errors.ServerError, "The command runner is not registered with service collection. command_name=name1 command_id=id1 runner=PerpetualIntelligence.Cli.Commands.Handlers.Mocks.MockRunner");
        }

        [TestMethod]
        public async Task RunnerErrorShouldFailHandler()
        {
            // Make sure checker pass so runner can fail
            command.Item1.Checker = typeof(MockChecker);

            command.Item1.Runner = typeof(MockErrorRunner);

            CommandHandlerContext commandHandler = new CommandHandlerContext(command.Item1, command.Item2);
            var result = await handler.HandleAsync(commandHandler);

            TestHelper.AssertOneImlxError(result, "test_runner_error", "test_runner_error_desc");
        }

        [TestMethod]
        public async Task RunnerNotConfiguredShouldError()
        {
            // Make sure checker pass so runner can fail
            command.Item1.Checker = typeof(MockChecker);

            CommandHandlerContext commandHandler = new CommandHandlerContext(command.Item1, command.Item2);
            var result = await handler.HandleAsync(commandHandler);
            TestHelper.AssertOneImlxError(result, Errors.ServerError, "The command runner is not configured. command_name=name1 command_id=id1");
        }

        [TestMethod]
        public async Task ValidCheckerAndRunnerShouldlAllowHandlerAsync()
        {
            command.Item1.Checker = typeof(MockChecker);
            command.Item1.Runner = typeof(MockRunner);

            CommandHandlerContext commandHandler = new CommandHandlerContext(command.Item1, command.Item2);
            var result = await handler.HandleAsync(commandHandler);

            Assert.IsFalse(result.IsError);
        }

        protected override void OnTestInitialize()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServices);
            host = hostBuilder.Build();

            command = MockCommands.NewCommand("id1", "name1", "prefix1");
            options = MockCliOptions.New();
            handler = new CommandHandler(host.Services, options, TestLogger.Create<CommandHandler>());
        }

        private void ConfigureCheckerOnly(IServiceCollection arg2)
        {
            arg2.AddTransient<MockChecker>();
        }

        private void ConfigureServices(IServiceCollection arg2)
        {
            arg2.AddTransient<MockChecker>();
            arg2.AddTransient<MockErrorChecker>();

            arg2.AddTransient<MockRunner>();
            arg2.AddTransient<MockErrorRunner>();

            arg2.AddTransient<MockNotCheckerOrRunner>();
        }

        private Tuple<CommandIdentity, Command> command = null!;
        private CommandHandler handler = null!;
        private IHost host = null!;
        private CliOptions options = null!;
    }
}
