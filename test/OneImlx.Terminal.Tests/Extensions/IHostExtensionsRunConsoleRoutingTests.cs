/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Extensions
{
    [Collection("Sequential")]
    public class IHostExtensionsRunConsoleRoutingTests : IAsyncLifetime
    {
        [Fact]
        public async Task Non_Console_Start_Mode_Throws_Invalid_Configuration()
        {
            // Cancel on first route so we can test user input without this we will go in infinite loop
            using IHost host = BuildHostAndLogger(ConfigureServicesCancelOnRoute);

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Set invalid start mode
            startContext = new TerminalStartContext(TerminalStartMode.Grpc, terminalTokenSource.Token, commandTokenSource.Token);
            Func<Task> act = async () => await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(new TerminalConsoleRouterContext(startContext));
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The requested start mode is not valid for console routing. start_mode=Grpc");
        }

        [Fact]
        public async Task RunConsoleRoutingShouldAskForUserInputAsync()
        {
            // Mock Console read and write
            Console.SetOut(consoleListWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route so we can test user input without this we will go in infinite loop
            using IHost host = BuildHostAndLogger(ConfigureServicesCancelOnRoute);

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(new TerminalConsoleRouterContext(startContext));

            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.RawCommandString.Should().Be("User has entered this command string");

            consoleListWriter.Messages.Should().ContainSingle(">");

            listLoggerFactory.AllLogMessages.Should().HaveCount(2);
            listLoggerFactory.AllLogMessages[0].Should().Be("Start terminal router. type=TerminalConsoleRouter context=TerminalConsoleRouterContext");
            listLoggerFactory.AllLogMessages[1].Should().Be("End terminal router.");
        }

        [Fact]
        public async Task RunConsoleRoutingShouldCancelOnRequestAsync()
        {
            // Mock Console read and write
            Console.SetOut(consoleListWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("does not matter");
            Console.SetIn(input);

            // Cancel on route
            using IHost host = BuildHostAndLogger(ConfigureServicesCancelOnRoute);

            // send cancellation after 2 seconds
            terminalTokenSource.CancelAfter(2000);

            // Wait for more than 2 seconds so task is canceled
            await Task.Delay(2050);

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(new TerminalConsoleRouterContext(startContext));

            // Canceled task so router will not be called.
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeFalse();

            // Check output
            MockExceptionPublisher errorPublisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            errorPublisher.Called.Should().BeTrue();
            errorPublisher.PublishedMessage.Should().Be("Received terminal cancellation token, the terminal routing is canceled.");

            consoleListWriter.Messages.Should().BeEmpty();

            listLoggerFactory.AllLogMessages.Should().HaveCount(2);
            listLoggerFactory.AllLogMessages[0].Should().Be("Start terminal router. type=TerminalConsoleRouter context=TerminalConsoleRouterContext");
            listLoggerFactory.AllLogMessages[1].Should().Be("End terminal router.");
        }

        [Fact]
        public async Task RunConsoleRoutingShouldHandleErrorExceptionCorrectlyAsync()
        {
            // Mock Console read and write
            Console.SetOut(consoleListWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            using IHost host = BuildHostAndLogger(ConfigureServicesErrorExceptionAndCancelOnRoute);

            // Router will throw exception and then routing will get canceled
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            GetCliOptions(host).Router.Caret = "$";
            await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(new TerminalConsoleRouterContext(startContext));

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            publisher.Called.Should().BeTrue();
            publisher.MultiplePublishedMessages.Count.Should().Be(2);
            publisher.MultiplePublishedMessages[0].Should().Be("test_error_description. opt1=test1 opt2=test2");
            publisher.MultiplePublishedMessages[1].Should().Be("Received terminal cancellation token, the terminal routing is canceled.");
            publisher.PublishedMessage.Should().Be("Received terminal cancellation token, the terminal routing is canceled.");

            consoleListWriter.Messages.Should().ContainSingle("$");

            listLoggerFactory.AllLogMessages.Should().HaveCount(2);
            listLoggerFactory.AllLogMessages[0].Should().Be("Start terminal router. type=TerminalConsoleRouter context=TerminalConsoleRouterContext");
            listLoggerFactory.AllLogMessages[1].Should().Be("End terminal router.");
        }

        [Fact]
        public async Task RunConsoleRoutingShouldHandleExplicitErrorCorrectlyAsync()
        {
            // Mock Console read and write
            Console.SetOut(consoleListWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            using IHost host = BuildHostAndLogger(ConfigureServicesExplicitErrorAndCancelOnRoute);

            // Router will throw exception and then routing will get canceled
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(new TerminalConsoleRouterContext(startContext));

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            publisher.Called.Should().BeTrue();
            publisher.MultiplePublishedMessages.Count.Should().Be(2);
            publisher.MultiplePublishedMessages[0].Should().Be("explicit_error_description param1=test_param1 param2=test_param2.");
            publisher.MultiplePublishedMessages[1].Should().Be("Received terminal cancellation token, the terminal routing is canceled.");
            publisher.PublishedMessage.Should().Be("Received terminal cancellation token, the terminal routing is canceled.");

            consoleListWriter.Messages.Should().ContainSingle(">");

            listLoggerFactory.AllLogMessages.Should().HaveCount(2);
            listLoggerFactory.AllLogMessages[0].Should().Be("Start terminal router. type=TerminalConsoleRouter context=TerminalConsoleRouterContext");
            listLoggerFactory.AllLogMessages[1].Should().Be("End terminal router.");
        }

        [Fact]
        public async Task RunConsoleRoutingShouldHandleHostStopCorrectlyAsync()
        {
            // Mock Console read and write
            Console.SetOut(consoleListWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("does not matter");
            Console.SetIn(input);

            // Cancel on route
            using IHost host = BuildHostAndLogger(ConfigureServicesDefault);
            await host.StartAsync();

            // Issue a callback after 2 seconds.
            Timer timer = new(HostStopRequestCallbackVoidAsync, host, 2000, Timeout.Infinite);

            // Run the router for 5 seconds, the callback will stop the host 2 seconds.
            GetCliOptions(host).Router.Timeout = 5000;
            await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(new TerminalConsoleRouterContext(startContext));

            // Till the timer callback cancel the route will be called multiple times.
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            publisher.Called.Should().BeTrue();
            publisher.MultiplePublishedMessages.Count.Should().Be(1);
            publisher.MultiplePublishedMessages[0].Should().Be("Application is stopping, the terminal routing is canceled.");
            publisher.PublishedMessage.Should().Be("Application is stopping, the terminal routing is canceled.");

            // Loop will add multiple carets.
            consoleListWriter.Messages.Distinct().Should().ContainSingle(">");

            // The StartAsync will also log the .NET hosting logging messages
            listLoggerFactory.AllLogMessages.Contains("Start terminal routing. router=TerminalConsoleRouter context=TerminalConsoleRouterContext");
            listLoggerFactory.AllLogMessages.Contains("End terminal routing.");
        }

        [Fact]
        public async Task RunConsoleRoutingShouldHandleRouteExceptionCorrectlyAsync()
        {
            // Mock Console read and write
            Console.SetOut(consoleListWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            using IHost host = BuildHostAndLogger(ConfigureServicesExceptionAndCancelOnRoute);

            // Router will throw exception and then routing will get canceled
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            GetCliOptions(host).Router.Caret = ">$";
            await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(new TerminalConsoleRouterContext(startContext));

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            publisher.Called.Should().BeTrue();
            publisher.MultiplePublishedMessages.Count.Should().Be(2);
            publisher.MultiplePublishedMessages[0].Should().Be("Test invalid operation.");
            publisher.MultiplePublishedMessages[1].Should().Be("Received terminal cancellation token, the terminal routing is canceled.");
            publisher.PublishedMessage.Should().Be("Received terminal cancellation token, the terminal routing is canceled.");

            consoleListWriter.Messages.Should().ContainSingle(">$");

            listLoggerFactory.AllLogMessages.Should().HaveCount(2);
            listLoggerFactory.AllLogMessages[0].Should().Be("Start terminal router. type=TerminalConsoleRouter context=TerminalConsoleRouterContext");
            listLoggerFactory.AllLogMessages[1].Should().Be("End terminal router.");
        }

        [Fact]
        public async Task RunConsoleRoutingShouldIgnoreEmptyInputAsync()
        {
            // Mock Console read and write
            Console.SetOut(consoleListWriter);

            // This mocks the empty command string entered by the user
            using var input = new StringReader("   ");
            Console.SetIn(input);

            using IHost host = BuildHostAndLogger(ConfigureServicesDefault);

            // We will run in a infinite loop due to empty input so break that after 2 seconds
            terminalTokenSource.CancelAfter(2000);
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(new TerminalConsoleRouterContext(startContext));

            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeFalse();

            // The log messages will have N raw string log messages
            string[] logMessages = listLoggerFactory.AllLogMessages.Distinct().ToArray();
            logMessages.Should().HaveCount(3);
            logMessages[0].Should().Be("Start terminal router. type=TerminalConsoleRouter context=TerminalConsoleRouterContext");
            logMessages[1].Should().Be("The raw string is null or ignored by the terminal console.");
            logMessages[2].Should().Be("End terminal router.");
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

            using IHost host = BuildHostAndLogger(ConfigureServicesDefault);

            // send cancellation after 3 seconds. Idea is that in 3 seconds the router will route multiple times till canceled.
            terminalTokenSource.CancelAfter(3000);
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(new TerminalConsoleRouterContext(startContext));

            // In 3 seconds the Route will be called multiple times.
            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.RouteCounter.Should().BeGreaterThan(10, $"This route counter {mockCommandRouter.RouteCounter} is just a guess, it should be called indefinitely till canceled.");

            // The log messages will have N raw string log messages
            string[] logMessages = listLoggerFactory.AllLogMessages.Distinct().ToArray();
            logMessages.Should().HaveCount(3);
            logMessages[0].Should().Be("Start terminal router. type=TerminalConsoleRouter context=TerminalConsoleRouterContext");
            logMessages[1].Should().Be("The raw string is null or ignored by the terminal console.");
            logMessages[2].Should().Be("End terminal router.");
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

            using IHost host = BuildHostAndLogger(ConfigureServicesDefault);

            MockCommandRouter mockCommandRouter = (MockCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.ReturnedRouterResult.Should().BeNull();

            // Send cancellation after 2 seconds. Idea is in 2 seconds the router will route multiple times till canceled.
            terminalTokenSource.CancelAfter(2000);
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(new TerminalConsoleRouterContext(startContext));

            // Result is processed and disposed by handler not the routing service.
            mockCommandRouter.ReturnedRouterResult.Should().NotBeNull();
            mockCommandRouter.ReturnedRouterResult!.HandlerResult.RunnerResult.IsProcessed.Should().BeFalse();
            mockCommandRouter.ReturnedRouterResult!.HandlerResult.RunnerResult.IsDisposed.Should().BeFalse();
        }

        [Fact]
        public async Task RunConsoleRoutingShouldTimeOutCorrectlyAsync()
        {
            // Mock Console read and write
            Console.SetOut(consoleListWriter);

            // This mocks the command string entered by the user
            using var input = new StringReader("User has entered this command string. ");
            Console.SetIn(input);

            // Cancel on first route and set delay so we can timeout and break the routing loop.
            using IHost host = BuildHostAndLogger(ConfigureServicesDelayAndCancelOnRoute);

            // Route delay is set to 3000 and timeout is 2000
            GetCliOptions(host).Router.Timeout = 2000;
            await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(new TerminalConsoleRouterContext(startContext));

            // Check the published error
            MockExceptionPublisher publisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            publisher.Called.Should().BeTrue();
            publisher.MultiplePublishedMessages.Count.Should().Be(2);
            publisher.MultiplePublishedMessages[0].Should().Be("The command router timed out in 2000 milliseconds.");
            publisher.MultiplePublishedMessages[1].Should().Be("Received terminal cancellation token, the terminal routing is canceled.");
            publisher.PublishedMessage.Should().Be("Received terminal cancellation token, the terminal routing is canceled.");

            consoleListWriter.Messages.Distinct().Should().ContainSingle(">");

            // The log messages will have N raw string log messages
            string[] logMessages = listLoggerFactory.AllLogMessages.Distinct().ToArray();
            logMessages.Should().HaveCount(3);
            logMessages[0].Should().Be("Start terminal router. type=TerminalConsoleRouter context=TerminalConsoleRouterContext");
            logMessages[1].Should().Be("The raw string is null or ignored by the terminal console.");
            logMessages[2].Should().Be("End terminal router.");
        }

        [Fact]
        public async Task RunConsoleRoutingCaretShouldBeSetCorrectlyAsync()
        {
            // Mock Console read and write
            using MockListWriter titleWriter = new();
            Console.SetOut(titleWriter);

            using var input = new StringReader("does not matter");
            Console.SetIn(input);

            // Cancel on first route
            using IHost host = BuildHostAndLogger(ConfigureServicesCancelOnRoute);

            // cancel the token after 2 seconds so routing will be called and it will raise an exception
            terminalTokenSource.CancelAfter(2000);
            GetCliOptions(host).Router.Timeout = Timeout.Infinite;
            GetCliOptions(host).Router.Caret = "test_caret";
            await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(new TerminalConsoleRouterContext(startContext));
            titleWriter.Messages.Should().ContainSingle("test_caret");

            // Check output
            MockExceptionPublisher errorPublisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            errorPublisher.Called.Should().BeTrue();
            errorPublisher.PublishedMessage.Should().NotBeNull();
            errorPublisher.PublishedMessage.Should().Be("Received terminal cancellation token, the terminal routing is canceled.");

            consoleListWriter.Messages.Should().BeEmpty();

            listLoggerFactory.AllLogMessages.Should().HaveCount(2);
            listLoggerFactory.AllLogMessages[0].Should().Be("Start terminal router. type=TerminalConsoleRouter context=TerminalConsoleRouterContext");
            listLoggerFactory.AllLogMessages[1].Should().Be("End terminal router.");
        }

        private void ConfigureServicesCancelOnRoute(IServiceCollection opt2)
        {
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();

            startContext = new TerminalStartContext(TerminalStartMode.Console, terminalTokenSource.Token, commandTokenSource.Token);

            opt2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, terminalTokenSource));
            opt2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            opt2.AddSingleton<ILoggerFactory>(new MockListLoggerFactory());
            opt2.AddLogging();

            // Add Exception publisher
            opt2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            opt2.AddSingleton<ITerminalRouter<TerminalConsoleRouterContext>, TerminalConsoleRouter>();
            opt2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private void ConfigureServicesDefault(IServiceCollection opt2)
        {
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();

            startContext = new TerminalStartContext(TerminalStartMode.Console, terminalTokenSource.Token, commandTokenSource.Token);

            opt2.AddSingleton<ICommandRouter>(new MockCommandRouter());
            opt2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            opt2.AddSingleton<ILoggerFactory>(new MockListLoggerFactory());
            opt2.AddLogging();

            // Add Exception publisher
            opt2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            opt2.AddSingleton<ITerminalRouter<TerminalConsoleRouterContext>, TerminalConsoleRouter>();
            opt2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private void ConfigureServicesDelayAndCancelOnRoute(IServiceCollection opt2)
        {
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();

            startContext = new TerminalStartContext(TerminalStartMode.Console, terminalTokenSource.Token, commandTokenSource.Token);

            opt2.AddSingleton<ICommandRouter>(new MockCommandRouter(3000, terminalTokenSource));
            opt2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            opt2.AddSingleton<ILoggerFactory>(new MockListLoggerFactory());
            opt2.AddLogging();

            // Add Exception publisher
            opt2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            opt2.AddSingleton<ITerminalRouter<TerminalConsoleRouterContext>, TerminalConsoleRouter>();
            opt2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private void ConfigureServicesErrorExceptionAndCancelOnRoute(IServiceCollection opt2)
        {
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();

            startContext = new TerminalStartContext(TerminalStartMode.Console, terminalTokenSource.Token, commandTokenSource.Token);

            opt2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, terminalTokenSource, new TerminalException("test_error_code", "test_error_description. opt1={0} opt2={1}", "test1", "test2")));
            opt2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            opt2.AddSingleton<ILoggerFactory>(new MockListLoggerFactory());
            opt2.AddLogging();

            // Add Exception publisher
            opt2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            opt2.AddSingleton<ITerminalRouter<TerminalConsoleRouterContext>, TerminalConsoleRouter>();
            opt2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private void ConfigureServicesExceptionAndCancelOnRoute(IServiceCollection opt2)
        {
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();

            startContext = new TerminalStartContext(TerminalStartMode.Console, terminalTokenSource.Token, commandTokenSource.Token);

            // Adding space at the end so that any msg are correctly appended.
            opt2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, terminalTokenSource, new InvalidOperationException("Test invalid operation.")));
            opt2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            opt2.AddSingleton<ILoggerFactory>(new MockListLoggerFactory());
            opt2.AddLogging();

            // Add Exception publisher
            opt2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            opt2.AddSingleton<ITerminalRouter<TerminalConsoleRouterContext>, TerminalConsoleRouter>();
            opt2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private void ConfigureServicesExplicitErrorAndCancelOnRoute(IServiceCollection opt2)
        {
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();

            startContext = new TerminalStartContext(TerminalStartMode.Console, terminalTokenSource.Token, commandTokenSource.Token);

            // Adding space at the end so that any msg are correctly appended.
            opt2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, terminalTokenSource, null, new OneImlx.Shared.Infrastructure.Error("explicit_error", "explicit_error_description param1={0} param2={1}.", "test_param1", "test_param2")));
            opt2.AddSingleton(MockTerminalOptions.NewLegacyOptions());

            // Tells the logger to write to string writer so we can test it,
            opt2.AddSingleton<ILoggerFactory>(new MockListLoggerFactory());
            opt2.AddLogging();

            // Add Exception publisher
            opt2.AddSingleton<IExceptionHandler>(new MockExceptionPublisher());

            // Add routing service
            opt2.AddSingleton<ITerminalRouter<TerminalConsoleRouterContext>, TerminalConsoleRouter>();
            opt2.AddSingleton<ITerminalConsole, TerminalSystemConsole>();
        }

        private static TerminalOptions GetCliOptions(IHost host)
        {
            return host.Services.GetRequiredService<TerminalOptions>();
        }

        private async void HostStopRequestCallbackVoidAsync(object? state)
        {
            IHost host = (IHost)state!;
            host.Should().NotBeNull();
            await host.StopAsync();
        }

        public Task InitializeAsync()
        {
            consoleListWriter = new MockListWriter();

            originalWriter = Console.Out;
            originalReader = Console.In;

            return Task.CompletedTask;
        }

        private IHost BuildHostAndLogger(Action<IServiceCollection> configureServicesDelegate)
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(configureServicesDelegate);
            IHost host = hostBuilder.Build();

            // Retrieve the logger to ensure it's created and stored for later assertions.
            listLoggerFactory = (MockListLoggerFactory)host.Services.GetRequiredService<ILoggerFactory>();

            return host;
        }

        public Task DisposeAsync()
        {
            // Reset console.
            Console.SetOut(originalWriter);
            Console.SetIn(originalReader);

            listLoggerFactory?.AllLogMessages.Clear();
            consoleListWriter?.Dispose();

            return Task.CompletedTask;
        }

        private MockListWriter consoleListWriter = null!;
        private MockListLoggerFactory listLoggerFactory = null!;
        private TextWriter originalWriter = null!;
        private TextReader originalReader = null!;
        private CancellationTokenSource terminalTokenSource = null!;
        private CancellationTokenSource commandTokenSource = null!;
        private TerminalStartContext startContext = null!;
    }
}