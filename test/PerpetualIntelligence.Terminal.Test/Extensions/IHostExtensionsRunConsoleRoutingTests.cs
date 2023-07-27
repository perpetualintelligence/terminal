/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Runtime;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Extensions
{
    [Collection("Sequential")]
    public class IHostExtensionsRunConsoleRoutingTests : IAsyncLifetime
    {
        public IHostExtensionsRunConsoleRoutingTests()
        {
            stringWriter = new StringWriter();
        }

        [Fact]
        public async Task Non_Console_Start_Mode_Throws_Invalid_Configuration()
        {
            // Cancel on first route so we can test user input without this we will go in infinite loop
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesCancelOnRoute);
            host = newhostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Set invalid start mode
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Grpc), tokenSource.Token);
            Func<Task> act = async () => await host.RunConsoleRoutingAsync(new ConsoleRoutingContext(startContext));
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The requested start mode is not valid for console routing. start_mode=Grpc");
        }

        [Fact]
        public async Task RunRouterAsTerminalShouldAskForUserInputAsync()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route so we can test user input without this we will go in infinite loop
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesCancelOnRoute);
            host = newhostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunConsoleRoutingAsync(new ConsoleRoutingContext(startContext));

            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.RawCommandString.Should().Be("User has entered this command string");
        }

        [Fact]
        public async Task RunRouterAsTerminalShouldCancelOnRequestAsync()
        {
            // Mock Console read and write
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
            await host.RunConsoleRoutingAsync(new ConsoleRoutingContext(startContext));

            // Canceled task so router will not be called.
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeFalse();

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            errorPublisher.Called.Should().BeTrue();
            errorPublisher.PublishedMessage.Should().Be("Received cancellation token, the routing is canceled.");
            stringWriter.ToString().Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public async Task RunRouterAsTerminalShouldHandleErrorExceptionCorrectlyAsync()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesErrorExceptionAndCancelOnRoute);
            host = newhostBuilder.Build();

            // Router will throw exception and then routing will get canceled
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            GetCliOptions(host).Router.Caret = "$";
            await host.RunConsoleRoutingAsync(new ConsoleRoutingContext(startContext));

            // Check the published error
            MockExceptionPublisher exPublisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            exPublisher.Called.Should().BeTrue();
            exPublisher.PublishedMessage.Should().Be("test_error_description. arg1=test1 arg2=test2");

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            errorPublisher.Called.Should().BeTrue();
            errorPublisher.PublishedMessage.Should().Be("Received cancellation token, the routing is canceled.");
            new string(stringWriter.ToString().Distinct().ToArray()).Should().Be("$");
        }

        [Fact]
        public async Task RunRouterAsTerminalShouldHandleExplicitErrorCorrectlyAsync()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesExplicitErrorAndCancelOnRoute);
            host = newhostBuilder.Build();

            // Router will throw exception and then routing will get canceled
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunConsoleRoutingAsync(new ConsoleRoutingContext(startContext));

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            publisher.Called.Should().BeTrue();
            publisher.PublishedMessage.Should().Be("explicit_error_description param1=test_param1 param2=test_param2.");

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            errorPublisher.Called.Should().BeTrue();
            errorPublisher.PublishedMessage.Should().Be("Received cancellation token, the routing is canceled.");
            new string(stringWriter.ToString().Distinct().ToArray()).Should().Be(">");
        }

        [Fact]
        public async Task RunRouterAsTerminalShouldHandleHostStopCorrectlyAsync()
        {
            // Mock Console read and write
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
            await host.RunConsoleRoutingAsync(new ConsoleRoutingContext(startContext));

            // Till the timer callback cancel the route will be called multiple times.
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            errorPublisher.Called.Should().BeTrue();
            errorPublisher.PublishedMessage.Should().NotBeNull();
            errorPublisher.PublishedMessage.Should().Be("Application is stopping, the routing is canceled.");
            stringWriter.ToString().Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task RunRouterAsTerminalShouldHandleRouteExceptionCorrectlyAsync()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesExceptionAndCancelOnRoute);
            host = newhostBuilder.Build();

            // Router will throw exception and then routing will get canceled
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            GetCliOptions(host).Router.Caret = ">$";
            await host.RunConsoleRoutingAsync(new ConsoleRoutingContext(startContext));

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            publisher.Called.Should().BeTrue();
            publisher.PublishedMessage.Should().Be("Test invalid operation.");

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            errorPublisher.Called.Should().BeTrue();
            errorPublisher.PublishedMessage.Should().NotBeNull();
            errorPublisher.PublishedMessage.Should().Be("Received cancellation token, the routing is canceled.");
            new string(stringWriter.ToString().Distinct().ToArray()).Should().Be(">$");
        }

        [Fact]
        public async Task RunRouterAsTerminalShouldIgnoreEmptyInputAsync()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // This mocks the empty command string entered by the user
            using var input = new StringReader("   ");
            Console.SetIn(input);

            // We will run in a infinite loop due to empty input so break that after 2 seconds
            tokenSource.CancelAfter(2000);
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunConsoleRoutingAsync(new ConsoleRoutingContext(startContext));

            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeFalse();
        }

        [Fact]
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
            await host.RunConsoleRoutingAsync(new ConsoleRoutingContext(startContext));

            // In 3 seconds the Route will be called multiple times.
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.RouteCounter.Should().BeGreaterThan(10, $"This route counter {mockCommandRouter.RouteCounter} is just a guess, it should be called indefinitely till canceled.");
        }

        [Fact]
        public async Task RunRouterAsTerminalShouldDisposeCommandRunnerResultAsync()
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

            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.ReturnedRouterResult.Should().BeNull();

            // send cancellation after 3 seconds. Idea is that in 3 seconds the router will route multiple times till canceled.
            tokenSource.CancelAfter(2000);
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunConsoleRoutingAsync(new ConsoleRoutingContext(startContext));

            mockCommandRouter.ReturnedRouterResult.Should().NotBeNull();
            mockCommandRouter.ReturnedRouterResult!.HandlerResult.RunnerResult.IsDisposed.Should().BeTrue();
        }

        [Fact]
        public async Task RunRouterAsTerminalShouldTimeOutCorrectlyAsync()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string. ");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDelayAndCancelOnRoute);
            host = newhostBuilder.Build();

            // Route delay is set to 3000 and timeout is 2000
            GetCliOptions(host).Router.Timeout = 2000;
            await host.RunConsoleRoutingAsync(new ConsoleRoutingContext(startContext));

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            publisher.Called.Should().BeTrue();
            publisher.PublishedMessage.Should().Be("The command router timed out in 2000 milliseconds.");

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            errorPublisher.Called.Should().BeTrue();
            errorPublisher.PublishedMessage.Should().NotBeNull();
            errorPublisher.PublishedMessage.Should().Be("Received cancellation token, the routing is canceled.");
            new string(stringWriter.ToString().Distinct().ToArray()).Should().Be(">");
        }

        [Fact]
        public async Task RunRouterAsTerminalCaretShouldBeSetCorrectlyAsync()
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
            GetCliOptions(host).Router.Caret = "test_caret";
            await host.RunConsoleRoutingAsync(new ConsoleRoutingContext(startContext));
            titleWriter.ToString().Should().Be("test_caret");

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            errorPublisher.Called.Should().BeTrue();
            errorPublisher.PublishedMessage.Should().NotBeNull();
            errorPublisher.PublishedMessage.Should().Be("Received cancellation token, the routing is canceled.");
            stringWriter.Should().NotBeNull();
            stringWriter.ToString().Should().BeNullOrWhiteSpace();
        }

        private void ConfigureServicesCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Console), tokenSource.Token);

            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, tokenSource));
            arg2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            var loggerFactory = new MockLoggerFactory
            {
                StringWriter = stringWriter
            };
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            arg2.AddSingleton<ConsoleRouting>();
        }

        private void ConfigureServicesDefault(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Console), tokenSource.Token);

            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter());
            arg2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            var loggerFactory = new MockLoggerFactory();
            loggerFactory.StringWriter = stringWriter;
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            arg2.AddSingleton<ConsoleRouting>();
        }

        private void ConfigureServicesDelayAndCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Console), tokenSource.Token);

            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter(3000, tokenSource));
            arg2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            var loggerFactory = new MockLoggerFactory
            {
                StringWriter = stringWriter
            };
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            arg2.AddSingleton<ConsoleRouting>();
        }

        private void ConfigureServicesErrorExceptionAndCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Console), tokenSource.Token);

            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, tokenSource, new ErrorException("test_error_code", "test_error_description. arg1={0} arg2={1}", "test1", "test2")));
            arg2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            var loggerFactory = new MockLoggerFactory
            {
                StringWriter = stringWriter
            };
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            arg2.AddSingleton<ConsoleRouting>();
        }

        private void ConfigureServicesExceptionAndCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Console), tokenSource.Token);

            // Adding space at the end so that any msg are correctly appended.
            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, tokenSource, new InvalidOperationException("Test invalid operation.")));
            arg2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            var loggerFactory = new MockLoggerFactory
            {
                StringWriter = stringWriter
            };
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            arg2.AddSingleton<ConsoleRouting>();
        }

        private void ConfigureServicesExplicitErrorAndCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Console), tokenSource.Token);

            // Adding space at the end so that any msg are correctly appended.
            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, tokenSource, null, new Shared.Infrastructure.Error("explicit_error", "explicit_error_description param1={0} param2={1}.", "test_param1", "test_param2")));
            arg2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            var loggerFactory = new MockLoggerFactory
            {
                StringWriter = stringWriter
            };
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            arg2.AddSingleton<ConsoleRouting>();
        }

        private TerminalOptions GetCliOptions(IHost host)
        {
            return host.Services.GetRequiredService<TerminalOptions>();
        }

        private void HostStopRequestCallback(object? state)
        {
            IHost? host = state as IHost;
            host.Should().NotBeNull();
            host!.StopAsync().GetAwaiter().GetResult();
        }

        public Task InitializeAsync()
        {
            originalWriter = Console.Out;
            originalReader = Console.In;

            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = hostBuilder.Build();

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            // Reset console.
            Console.SetOut(originalWriter);
            Console.SetIn(originalReader);

            if (host != null)
            {
                host.Dispose();
            }

            if (stringWriter != null)
            {
                stringWriter.Dispose();
            }

            return Task.CompletedTask;
        }

        private IHost host = null!;
        private StringWriter stringWriter;
        private TextWriter originalWriter = null!;
        private TextReader originalReader = null!;
        private CancellationTokenSource tokenSource = null!;
        private TerminalStartContext startContext = null!;
    }
}