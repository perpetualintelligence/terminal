/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Routers.Mocks;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    [TestClass]
    public class CommandRouterTests : LogTest
    {
        public CommandRouterTests() : base(TestLogger.Create<CommandRouterTests>())
        {
        }

        [TestMethod]
        public async Task ExtractorErrorShouldNotRouteFurtherAsync()
        {
            extractor.IsExplicitError = true;

            CommandRouterContext routerContext = new CommandRouterContext("test_command_string");

            await TestHelper.AssertThrowsErrorExceptionAsync(() => router.RouteAsync(routerContext), "test_extractor_error", "test_extractor_error_desc");
            Assert.IsTrue(extractor.Called);
            Assert.IsFalse(handler.Called);
        }

        [TestMethod]
        public async Task ExtractorNoExtractedCommandIdentityShouldNotRouteFurtherAsync()
        {
            extractor.IsExplicitNoCommandIdenitity = true;

            CommandRouterContext routerContext = new("test_command_string");

            await TestHelper.AssertThrowsWithMessageAsync<ArgumentException>(() => router.RouteAsync(routerContext), "Value cannot be null. (Parameter 'commandDescriptor')");
            Assert.IsTrue(extractor.Called);
            Assert.IsFalse(handler.Called);
        }

        [TestMethod]
        public async Task ExtractorNoExtractedCommandShouldNotRouteFurtherAsync()
        {
            extractor.IsExplicitNoCommand = true;

            CommandRouterContext routerContext = new CommandRouterContext("test_command_string");

            await TestHelper.AssertThrowsWithMessageAsync<ArgumentException>(() => router.RouteAsync(routerContext), "Value cannot be null. (Parameter 'command')");
            Assert.IsTrue(extractor.Called);
            Assert.IsFalse(handler.Called);
        }

        [TestMethod]
        public async Task HandlerErrorShouldNotRouteFurtherAsync()
        {
            handler.IsExplicitError = true;

            CommandRouterContext routerContext = new("test_command_string");

            await TestHelper.AssertThrowsErrorExceptionAsync(() => router.RouteAsync(routerContext), "test_handler_error", "test_handler_error_desc");
            Assert.IsTrue(handler.Called);
        }

        [TestMethod]
        public async Task RouterShouldCallExtractorAsync()
        {
            CommandRouterContext routerContext = new("test_command_string");
            var result = await router.RouteAsync(routerContext);
            Assert.IsTrue(extractor.Called);
        }

        [TestMethod]
        public async Task RouterShouldCallHandlerAfterExtractorAsync()
        {
            CommandRouterContext routerContext = new("test_command_string");
            var result = await router.RouteAsync(routerContext); ;

            Assert.IsTrue(extractor.Called);
            Assert.IsTrue(handler.Called);
        }

        [TestMethod]
        public async Task RouterShouldFindHandler()
        {
            CommandRouterContext routerContext = new("test_command_string");
            var result = await router.TryFindHandlerAsync(routerContext);

            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Result);
            Assert.IsInstanceOfType(result.Result, typeof(MockHandler));
            Assert.IsTrue(ReferenceEquals(handler, result.Result));
            Assert.IsFalse(handler.Called);
        }

        [TestMethod]
        public async Task RouterShouldPassExtracterCommandToHandlerAsync()
        {
            CommandRouterContext routerContext = new("test_command_string");
            var result = await router.RouteAsync(routerContext);

            Assert.IsNotNull(handler.ContextCalled);
            Assert.AreEqual("test_id", handler.ContextCalled.CommandDescriptor.Id);
            Assert.AreEqual("test_name", handler.ContextCalled.CommandDescriptor.Name);
            Assert.AreEqual("test_prefix", handler.ContextCalled.CommandDescriptor.Prefix);

            Assert.AreEqual("test_id", handler.ContextCalled.Command.Id);
            Assert.AreEqual("test_name", handler.ContextCalled.Command.Name);
        }

        [TestMethod]
        public void RouteWithNullArgumentsShouldThrow()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandRouter(null, null, null, null), "Value cannot be null. (Parameter 'extractor')");
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandRouter(extractor, null, null, null), "Value cannot be null. (Parameter 'handler')");
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandRouter(extractor, handler, null, null), "Value cannot be null. (Parameter 'options')");
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new CommandRouter(extractor, handler, options, null), "Value cannot be null. (Parameter 'logger')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        protected override void OnTestCleanup()
        {
            if (host != null)
            {
                host.Dispose();
            }
        }

        protected override void OnTestInitialize()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>());
            host = hostBuilder.Build();

            options = MockCliOptions.New();
            extractor = new MockExtractor();
            handler = new MockHandler();
            router = new CommandRouter(extractor, handler, options, TestLogger.Create<CommandRouter>());
        }

        private MockExtractor extractor = null!;
        private MockHandler handler = null!;
        private IHost host = null!;
        private CliOptions options = null!;
        private CommandRouter router = null!;
    }
}
