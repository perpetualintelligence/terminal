/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Extensions
{
    [TestClass]
    public class IHostExtensionsRunAsTerminalTests : InitializerTests
    {
        public IHostExtensionsRunAsTerminalTests() : base(TestLogger.Create<IHostExtensionsRunAsTerminalTests>())
        {
        }

        [TestMethod]
        public async Task RunRouterAsTerminalShouldAskForUserInputAsync()
        {
            // Mock Console read and write
            Assert.IsNotNull(stringWriter);
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route so we can test user input without this we will go in infinite loop
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesCancelOnRoute);
            host = newhostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunRouterAsTerminalAsync("test_title", tokenSource.Token);

            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            Assert.IsTrue(mockCommandRouter.RouteCalled);
            Assert.AreEqual("User has entered this command string", mockCommandRouter.RawCommandString);
        }

        [TestMethod]
        public async Task RunRouterAsTerminalShouldCancelOnRequestAsync()
        {
            // Mock Console read and write
            Assert.IsNotNull(stringWriter);
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("does not matter");
            Console.SetIn(input);

            // Cancel on route
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesCancelOnRoute);
            host = newhostBuilder.Build();

            // send cancellation after 2 seconds
            tokenSource.CancelAfter(2000);

            // Wait for more than 2.05 seconds so task is canceled
            await Task.Delay(2050);

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunRouterAsTerminalAsync("test_title", tokenSource.Token);

            // Canceled task so router will not be called.
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            Assert.IsFalse(mockCommandRouter.RouteCalled);

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            Assert.IsTrue(errorPublisher.Called);
            Assert.AreEqual("Received cancellation token, the routing is canceled.", errorPublisher.PublishedMessage);
            Assert.IsTrue(string.IsNullOrWhiteSpace(stringWriter.ToString()));
        }

        [TestMethod]
        public async Task RunRouterAsTerminalShouldHandleErrorExceptionCorrectlyAsync()
        {
            // Mock Console read and write
            Assert.IsNotNull(stringWriter);
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesErrorExceptionAndCancelOnRoute);
            host = newhostBuilder.Build();

            // Router will throw exception and then routing will get canceled
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunRouterAsTerminalAsync("test_title", tokenSource.Token);

            // Check the published error
            MockExceptionPublisher exPublisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            Assert.IsTrue(exPublisher.Called);
            Assert.AreEqual("test_error_description. arg1=test1 arg2=test2", exPublisher.PublishedMessage);

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            Assert.IsTrue(errorPublisher.Called);
            Assert.AreEqual("Received cancellation token, the routing is canceled.", errorPublisher.PublishedMessage);
            Assert.IsTrue(string.IsNullOrWhiteSpace(stringWriter.ToString()));
        }

        [TestMethod]
        public async Task RunRouterAsTerminalShouldHandleExplictErrorCorrectlyAsync()
        {
            // Mock Console read and write
            Assert.IsNotNull(stringWriter);
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesExplicitErrorAndCancelOnRoute);
            host = newhostBuilder.Build();

            // Router will throw exception and then routing will get canceled
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunRouterAsTerminalAsync("test_title", tokenSource.Token);

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            Assert.IsTrue(publisher.Called);
            Assert.AreEqual("explicit_error_description param1=test_param1 param2=test_param2.", publisher.PublishedMessage);

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            Assert.IsTrue(errorPublisher.Called);
            Assert.AreEqual("Received cancellation token, the routing is canceled.", errorPublisher.PublishedMessage);
            Assert.IsTrue(string.IsNullOrWhiteSpace(stringWriter.ToString()));
        }

        [TestMethod]
        public async Task RunRouterAsTerminalShouldHandleHostStopCorrectlyAsync()
        {
            // Mock Console read and write
            Assert.IsNotNull(stringWriter);
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("does not matter");
            Console.SetIn(input);

            // Cancel on route
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = newhostBuilder.Build();
            await host.StartAsync();

            // Issue a callback after 2 seconds.
            Timer timer = new(HostStopRequestCallback, host, 2000, Timeout.Infinite);

            // Run the router for 5 seconds, the callback will stop the host 2 seconds.
            GetCliOptions(host).Router.Timeout = 5000;
            await host.RunRouterAsTerminalAsync("test_title", tokenSource.Token);

            // Till the timer callback cancel the route will be called multiple times.
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            Assert.IsTrue(mockCommandRouter.RouteCalled);

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            Assert.IsTrue(errorPublisher.Called);
            Assert.IsNotNull(errorPublisher.PublishedMessage);
            Assert.AreEqual("Application is stopping, the routing is canceled.", errorPublisher.PublishedMessage);
            Assert.IsFalse(string.IsNullOrWhiteSpace(stringWriter.ToString()));
        }

        [TestMethod]
        public async Task RunRouterAsTerminalShouldHandleRouteExceptionCorrectlyAsync()
        {
            // Mock Console read and write
            Assert.IsNotNull(stringWriter);
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesExceptionAndCancelOnRoute);
            host = newhostBuilder.Build();

            // Router will throw exception and then routing will get canceled
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunRouterAsTerminalAsync("test_title", tokenSource.Token);

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            Assert.IsTrue(publisher.Called);
            Assert.AreEqual("Test invalid operation.", publisher.PublishedMessage);

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            Assert.IsTrue(errorPublisher.Called);
            Assert.IsNotNull(errorPublisher.PublishedMessage);
            Assert.AreEqual("Received cancellation token, the routing is canceled.", errorPublisher.PublishedMessage);
            Assert.IsTrue(string.IsNullOrWhiteSpace(stringWriter.ToString()));
        }

        [TestMethod]
        public async Task RunRouterAsTerminalShouldIgnoreEmptyInputAsync()
        {
            // Mock Console read and write
            Assert.IsNotNull(stringWriter);
            Console.SetOut(stringWriter);

            // This mocks the empty command string entered by the user
            using var input = new StringReader("   ");
            Console.SetIn(input);

            // We will run in a infinite loop due to empty input so break that after 2 seconds
            tokenSource.CancelAfter(2000);
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunRouterAsTerminalAsync("test_title", tokenSource.Token);

            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            Assert.IsFalse(mockCommandRouter.RouteCalled);
        }

        [TestMethod]
        public async Task RunRouterAsTerminalShouldRunIdefinatelyTillCancelAsync()
        {
            // Mock Console read and write
            using var output = new StringWriter();
            Console.SetOut(output);

            // Mock the multiple lines here so that RunRouterAsTerminalAsync can readline multiple times
            using var input = new StringReader("does not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter");
            Console.SetIn(input);

            // The default does not have and cancel or timeout
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = newhostBuilder.Build();

            // send cancellation after 3 seconds. Idea is that in 3 seconds the router will route multiple times till canceled.
            tokenSource.CancelAfter(3000);
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunRouterAsTerminalAsync("test_title", tokenSource.Token);

            // In 3 seconds the Route will be called miltiple times.
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            Assert.IsTrue(mockCommandRouter.RouteCalled);
            Assert.IsTrue(mockCommandRouter.RouteCounter > 10, $"This route counter {mockCommandRouter.RouteCounter} is just a guess, it should be called indefinately till cancelled.");
        }

        [TestMethod]
        public async Task RunRouterAsTerminalShouldTimeOutCorrectlyAsync()
        {
            // Mock Console read and write
            Assert.IsNotNull(stringWriter);
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string. ");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDelayAndCancelOnRoute);
            host = newhostBuilder.Build();

            // Route delay is set to 3000 and timeout is 2000
            GetCliOptions(host).Router.Timeout = 2000;
            await host.RunRouterAsTerminalAsync("test_title", tokenSource.Token);

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            Assert.IsTrue(publisher.Called);
            Assert.AreEqual("The command router timed out in 2000 milliseconds.", publisher.PublishedMessage);

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            Assert.IsTrue(errorPublisher.Called);
            Assert.IsNotNull(errorPublisher.PublishedMessage);
            Assert.AreEqual("Received cancellation token, the routing is canceled.", errorPublisher.PublishedMessage);
            Assert.IsTrue(string.IsNullOrWhiteSpace(stringWriter.ToString()));
        }

        [TestMethod]
        public async Task RunRouterAsTerminalTitleShouldBeSetCorrectlyAsync()
        {
            // Mock Console read and write
            using StringWriter titleWriter = new StringWriter();
            Console.SetOut(titleWriter);

            using var input = new StringReader("does not matter");
            Console.SetIn(input);

            // Cancel on first route
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesCancelOnRoute);
            host = newhostBuilder.Build();

            // cancel the token after 2 seconds so routing will be called and it will raise an exception
            tokenSource.CancelAfter(2000);
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunRouterAsTerminalAsync("test_title", tokenSource.Token);
            Assert.AreEqual("test_title", titleWriter.ToString());

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            Assert.IsTrue(errorPublisher.Called);
            Assert.IsNotNull(errorPublisher.PublishedMessage);
            Assert.AreEqual("Received cancellation token, the routing is canceled.", errorPublisher.PublishedMessage);
            Assert.IsNotNull(stringWriter);
            Assert.IsTrue(string.IsNullOrWhiteSpace(stringWriter.ToString()));
        }

        protected override void OnTestCleanup()
        {
            if (host != null)
            {
                host.Dispose();
            }

            if (stringWriter != null)
            {
                stringWriter.Dispose();
            }
        }

        protected override void OnTestInitialize()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = hostBuilder.Build();
        }

        private void ConfigureServicesCancelledRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            arg2.AddSingleton<ICommandRouter>(new MockCommandRouterCancellation());
            arg2.AddSingleton(MockCliOptions.New());

            // Tells the logger to write to string writer so we can test it,
            stringWriter = new StringWriter();
            var loggerFactory = new MockLoggerFactory();
            loggerFactory.StringWriter = stringWriter;
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());
        }

        private void ConfigureServicesCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, tokenSource));
            arg2.AddSingleton(MockCliOptions.New());

            // Tells the logger to write to string writer so we can test it,
            stringWriter = new StringWriter();
            var loggerFactory = new MockLoggerFactory();
            loggerFactory.StringWriter = stringWriter;
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());
        }

        private void ConfigureServicesDefault(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter());
            arg2.AddSingleton(MockCliOptions.New());

            // Tells the logger to write to string writer so we can test it,
            stringWriter = new StringWriter();
            var loggerFactory = new MockLoggerFactory();
            loggerFactory.StringWriter = stringWriter;
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());
        }

        private void ConfigureServicesDelayAndCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter(3000, tokenSource));
            arg2.AddSingleton(MockCliOptions.New());

            // Tells the logger to write to string writer so we can test it,
            stringWriter = new StringWriter();
            var loggerFactory = new MockLoggerFactory
            {
                StringWriter = stringWriter
            };
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());
        }

        private void ConfigureServicesErrorExceptionAndCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();

            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, tokenSource, new ErrorException("test_error_code", "test_error_description. arg1={0} arg2={1}", "test1", "test2")));
            arg2.AddSingleton(MockCliOptions.New());

            // Tells the logger to write to string writer so we can test it,
            stringWriter = new StringWriter();
            var loggerFactory = new MockLoggerFactory
            {
                StringWriter = stringWriter
            };
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());
        }

        private void ConfigureServicesExceptionAndCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();

            // Adding space at the end so that any msg are correctly appended.
            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, tokenSource, new InvalidOperationException("Test invalid operation.")));
            arg2.AddSingleton(MockCliOptions.New());

            // Tells the logger to write to string writer so we can test it,
            stringWriter = new StringWriter();
            var loggerFactory = new MockLoggerFactory
            {
                StringWriter = stringWriter
            };
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());
        }

        private void ConfigureServicesExplicitErrorAndCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();

            // Adding space at the end so that any msg are correctly appended.
            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, tokenSource, null, new Shared.Infrastructure.Error("explicit_error", "explicit_error_description param1={0} param2={1}.", "test_param1", "test_param2")));
            arg2.AddSingleton(MockCliOptions.New());

            // Tells the logger to write to string writer so we can test it,
            stringWriter = new StringWriter();
            var loggerFactory = new MockLoggerFactory
            {
                StringWriter = stringWriter
            };
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());
        }

        private CliOptions GetCliOptions(IHost host)
        {
            return host.Services.GetRequiredService<CliOptions>();
        }

        private void HostStopRequestCallback(object? state)
        {
            IHost? host = state as IHost;
            Assert.IsNotNull(host);
            host.StopAsync().GetAwaiter().GetResult();
        }

        private IHost host = null!;
        private StringWriter? stringWriter = null!;
        private CancellationTokenSource tokenSource = null!;
    }
}