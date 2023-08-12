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
        [Fact]
        public async Task Non_Console_Start_Mode_Throws_Invalid_Configuration()
        {
            // Cancel on first route so we can test user input without this we will go in infinite loop
            var newHostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesCancelOnRoute);
            host = newHostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Set invalid start mode
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Grpc), tokenSource.Token);
            Func<Task> act = async () => await host.RunConsoleRoutingAsync(new TerminalConsoleRoutingContext(startContext));
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The requested start mode is not valid for console routing. start_mode=Grpc");
        }

        [Fact]
        public async Task RunConsoleRoutingShouldAskForUserInputAsync()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route so we can test user input without this we will go in infinite loop
            var newHostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesCancelOnRoute);
            host = newHostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunConsoleRoutingAsync(new TerminalConsoleRoutingContext(startContext));

            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.RawCommandString.Should().Be("User has entered this command string");
        }

        [Fact]
        public async Task RunConsoleRoutingShouldCancelOnRequestAsync()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("does not matter");
            Console.SetIn(input);

            // Cancel on route
            var newHostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesCancelOnRoute);
            host = newHostBuilder.Build();

            // send cancellation after 2 seconds
            tokenSource.CancelAfter(2000);

            // Wait for more than 2.05 seconds so task is canceled
            await Task.Delay(2050);

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunConsoleRoutingAsync(new TerminalConsoleRoutingContext(startContext));

            // Canceled task so router will not be called.
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeFalse();

            // Check output
            MockExceptionPublisher errorPublisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            errorPublisher.Called.Should().BeTrue();
            errorPublisher.PublishedMessage.Should().Be("Received cancellation token, the routing is canceled.");
            stringWriter.ToString().Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public async Task RunConsoleRoutingShouldHandleErrorExceptionCorrectlyAsync()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            var newHostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesErrorExceptionAndCancelOnRoute);
            host = newHostBuilder.Build();

            // Router will throw exception and then routing will get canceled
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            GetCliOptions(host).Router.Caret = "$";
            await host.RunConsoleRoutingAsync(new TerminalConsoleRoutingContext(startContext));

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            publisher.Called.Should().BeTrue();
            publisher.MultiplePublishedMessages.Count.Should().Be(2);
            publisher.MultiplePublishedMessages[0].Should().Be("test_error_description. arg1=test1 arg2=test2");
            publisher.MultiplePublishedMessages[1].Should().Be("Received cancellation token, the routing is canceled.");
            publisher.PublishedMessage.Should().Be("Received cancellation token, the routing is canceled.");
            new string(stringWriter.ToString().Distinct().ToArray()).Should().Be("$");
        }

        [Fact]
        public async Task RunConsoleRoutingShouldHandleExplicitErrorCorrectlyAsync()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            var newHostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesExplicitErrorAndCancelOnRoute);
            host = newHostBuilder.Build();

            // Router will throw exception and then routing will get canceled
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunConsoleRoutingAsync(new TerminalConsoleRoutingContext(startContext));

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            publisher.Called.Should().BeTrue();
            publisher.MultiplePublishedMessages.Count.Should().Be(2);
            publisher.MultiplePublishedMessages[0].Should().Be("explicit_error_description param1=test_param1 param2=test_param2.");
            publisher.MultiplePublishedMessages[1].Should().Be("Received cancellation token, the routing is canceled.");
            publisher.PublishedMessage.Should().Be("Received cancellation token, the routing is canceled.");
            new string(stringWriter.ToString().Distinct().ToArray()).Should().Be(">");
        }

        [Fact]
        public async Task RunConsoleRoutingShouldHandleHostStopCorrectlyAsync()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("does not matter");
            Console.SetIn(input);

            // Cancel on route
            var newHostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();
            await host.StartAsync();

            // Issue a callback after 2 seconds.
            Timer timer = new(HostStopRequestCallback, host, 2000, Timeout.Infinite);

            // Run the router for 5 seconds, the callback will stop the host 2 seconds.
            GetCliOptions(host).Router.Timeout = 5000;
            await host.RunConsoleRoutingAsync(new TerminalConsoleRoutingContext(startContext));

            // Till the timer callback cancel the route will be called multiple times.
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            publisher.Called.Should().BeTrue();
            publisher.MultiplePublishedMessages.Count.Should().Be(1);
            publisher.MultiplePublishedMessages[0].Should().Be("Application is stopping, the routing is canceled.");
            publisher.PublishedMessage.Should().Be("Application is stopping, the routing is canceled.");
            stringWriter.ToString().Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task RunConsoleRoutingShouldHandleRouteExceptionCorrectlyAsync()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            var newHostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesExceptionAndCancelOnRoute);
            host = newHostBuilder.Build();

            // Router will throw exception and then routing will get canceled
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            GetCliOptions(host).Router.Caret = ">$";
            await host.RunConsoleRoutingAsync(new TerminalConsoleRoutingContext(startContext));

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            publisher.Called.Should().BeTrue();
            publisher.MultiplePublishedMessages.Count.Should().Be(2);
            publisher.MultiplePublishedMessages[0].Should().Be("Test invalid operation.");
            publisher.MultiplePublishedMessages[1].Should().Be("Received cancellation token, the routing is canceled.");
            publisher.PublishedMessage.Should().Be("Received cancellation token, the routing is canceled.");
            new string(stringWriter.ToString().Distinct().ToArray()).Should().Be(">$");
        }

        [Fact]
        public async Task RunConsoleRoutingShouldIgnoreEmptyInputAsync()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // This mocks the empty command string entered by the user
            using var input = new StringReader("   ");
            Console.SetIn(input);

            // We will run in a infinite loop due to empty input so break that after 2 seconds
            tokenSource.CancelAfter(2000);
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunConsoleRoutingAsync(new TerminalConsoleRoutingContext(startContext));

            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeFalse();
        }

        [Fact]
        public async Task RunConsoleRoutingShouldRunIndefinitelyTillCancelAsync()
        {
            // Mock Console read and write
            using var output = new StringWriter();
            Console.SetOut(output);

            // Mock the multiple lines here so that RunConsoleRoutingAsync can read line multiple times
            using var input = new StringReader("does not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter");
            Console.SetIn(input);

            // The default does not have and cancel or timeout
            var newHostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            // send cancellation after 3 seconds. Idea is that in 3 seconds the router will route multiple times till canceled.
            tokenSource.CancelAfter(3000);
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunConsoleRoutingAsync(new TerminalConsoleRoutingContext(startContext));

            // In 3 seconds the Route will be called multiple times.
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.RouteCounter.Should().BeGreaterThan(10, $"This route counter {mockCommandRouter.RouteCounter} is just a guess, it should be called indefinitely till canceled.");
        }

        [Fact]
        public async Task RunConsoleRouting_DoesNot_Process_Or_Dispose_RunnerResultAsync()
        {
            // Mock Console read and write
            using var output = new StringWriter();
            Console.SetOut(output);

            // Mock the multiple lines here so that RunConsoleRoutingAsync can read line multiple times
            using var input = new StringReader("does not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter\ndoes not matter");
            Console.SetIn(input);

            // The default does not have and cancel or timeout
            var newHostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = newHostBuilder.Build();

            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.ReturnedRouterResult.Should().BeNull();

            // Send cancellation after 2 seconds. Idea is in 2 seconds the router will route multiple times till canceled.
            tokenSource.CancelAfter(2000);
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunConsoleRoutingAsync(new TerminalConsoleRoutingContext(startContext));

            // Result is processed and disposed by handler not the routing service.
            mockCommandRouter.ReturnedRouterResult.Should().NotBeNull();
            mockCommandRouter.ReturnedRouterResult!.HandlerResult.RunnerResult.IsProcessed.Should().BeFalse();
            mockCommandRouter.ReturnedRouterResult!.HandlerResult.RunnerResult.IsDisposed.Should().BeFalse();
        }

        [Fact]
        public async Task RunConsoleRoutingShouldTimeOutCorrectlyAsync()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string. ");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            var newHostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDelayAndCancelOnRoute);
            host = newHostBuilder.Build();

            // Route delay is set to 3000 and timeout is 2000
            GetCliOptions(host).Router.Timeout = 2000;
            await host.RunConsoleRoutingAsync(new TerminalConsoleRoutingContext(startContext));

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            publisher.Called.Should().BeTrue();
            publisher.MultiplePublishedMessages.Count.Should().Be(2);
            publisher.MultiplePublishedMessages[0].Should().Be("The command router timed out in 2000 milliseconds.");
            publisher.MultiplePublishedMessages[1].Should().Be("Received cancellation token, the routing is canceled.");
            publisher.PublishedMessage.Should().Be("Received cancellation token, the routing is canceled.");
            new string(stringWriter.ToString().Distinct().ToArray()).Should().Be(">");
        }

        [Fact]
        public async Task RunConsoleRoutingCaretShouldBeSetCorrectlyAsync()
        {
            // Mock Console read and write
            using StringWriter titleWriter = new();
            Console.SetOut(titleWriter);

            using var input = new StringReader("does not matter");
            Console.SetIn(input);

            // Cancel on first route
            var newHostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesCancelOnRoute);
            host = newHostBuilder.Build();

            // cancel the token after 2 seconds so routing will be called and it will raise an exception
            tokenSource.CancelAfter(2000);
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            GetCliOptions(host).Router.Caret = "test_caret";
            await host.RunConsoleRoutingAsync(new TerminalConsoleRoutingContext(startContext));
            titleWriter.ToString().Should().Be("test_caret");

            // Check output
            MockExceptionPublisher errorPublisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
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
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            arg2.AddSingleton<TerminalConsoleRouting>();
            arg2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private void ConfigureServicesDefault(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Console), tokenSource.Token);

            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter());
            arg2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            arg2.AddSingleton<TerminalConsoleRouting>();
            arg2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private void ConfigureServicesDelayAndCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Console), tokenSource.Token);

            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter(3000, tokenSource));
            arg2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            arg2.AddSingleton<TerminalConsoleRouting>();
            arg2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private void ConfigureServicesErrorExceptionAndCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Console), tokenSource.Token);

            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, tokenSource, new ErrorException("test_error_code", "test_error_description. arg1={0} arg2={1}", "test1", "test2")));
            arg2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            arg2.AddSingleton<TerminalConsoleRouting>();
            arg2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private void ConfigureServicesExceptionAndCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Console), tokenSource.Token);

            // Adding space at the end so that any msg are correctly appended.
            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, tokenSource, new InvalidOperationException("Test invalid operation.")));
            arg2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            arg2.AddSingleton<TerminalConsoleRouting>();
            arg2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private void ConfigureServicesExplicitErrorAndCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            startContext = new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Console), tokenSource.Token);

            // Adding space at the end so that any msg are correctly appended.
            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, tokenSource, null, new Shared.Infrastructure.Error("explicit_error", "explicit_error_description param1={0} param2={1}.", "test_param1", "test_param2")));
            arg2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Exception publisher
            arg2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            arg2.AddSingleton<TerminalConsoleRouting>();
            arg2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private static TerminalOptions GetCliOptions(IHost host)
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
            stringWriter = new StringWriter();

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

            host?.Dispose();

            stringWriter?.Dispose();

            return Task.CompletedTask;
        }

        private IHost host = null!;
        private StringWriter stringWriter = null!;
        private TextWriter originalWriter = null!;
        private TextReader originalReader = null!;
        private CancellationTokenSource tokenSource = null!;
        private TerminalStartContext startContext = null!;
    }
}