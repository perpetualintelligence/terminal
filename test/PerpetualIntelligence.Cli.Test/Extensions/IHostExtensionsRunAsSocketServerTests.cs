/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Cli.Extensions
{
    [Collection("Sequential")]
    public class IHostExtensionsRunAsSocketServerTests : InitializerTests, IAsyncLifetime
    {
        public IHostExtensionsRunAsSocketServerTests() : base(TestLogger.Create<IHostExtensionsRunAsSocketServerTests>())
        {
            stringWriter = new StringWriter();
        }

        [Fact]
        public void ShouldContinuteInfiniteLoopIfNotCancelled()
        {
            // Cancel on first route so we can test socket disposed we will go in infinite loop
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = newhostBuilder.Build();

            // Start sender and receiver communication and wait for 5 secs
            Task routerTask = host.RunRouterAsSocketServer(listenerSocketInitializerDelegate, SocketFlags.None, new IPEndPoint(IPAddress.Loopback, 12345), 5);
            Task senderTask = SendMessageOverSocket("Test message from sender", 12345);
            bool complete = Task.WaitAll(new Task[] { routerTask, senderTask }, 5000);

            // 5 sec timeour but not complete
            complete.Should().BeFalse();
        }

        [Fact]
        public void ShouldBreakInfiniteLoopIfCancelled()
        {
            // Cancel on first route so we can test socket disposed we will go in infinite loop
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesCancelOnRoute);
            host = newhostBuilder.Build();

            // Start sender and receiver communication and wait for 5 secs
            Task routerTask = host.RunRouterAsSocketServer(listenerSocketInitializerDelegate, SocketFlags.None, new IPEndPoint(IPAddress.Loopback, 12345), 5, tokenSource.Token);
            Task senderTask = SendMessageOverSocket("Test message from sender", 12345);
            bool complete = Task.WaitAll(new Task[] { routerTask, senderTask }, 5000);

            // complete within 5 sec timeour
            complete.Should().BeTrue();

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            errorPublisher.Called.Should().BeTrue();
            errorPublisher.PublishedMessage.Should().Be("Received cancellation token, the routing is canceled.");
            stringWriter.ToString().Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public void RouterShouldBeCalled()
        {
            // Cancel on first route so we can test socket disposed we will go in infinite loop
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = newhostBuilder.Build();

            // Start sender and receiver communication and wait for 5 secs
            Task routerTask = host.RunRouterAsSocketServer(listenerSocketInitializerDelegate, SocketFlags.None, new IPEndPoint(IPAddress.Loopback, 12345), 5, tokenSource.Token);
            Task senderTask = SendMessageOverSocket("Test message from sender", 12345);
            bool complete = Task.WaitAll(new Task[] { routerTask, senderTask }, 5000);
            complete.Should().BeFalse();

            // Verify router called
            MockSocketCommandRouter mockCommandRouter = (MockSocketCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.RawCommandStrings.Should().ContainSingle("Test message from sender");

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            errorPublisher.Called.Should().BeFalse();
        }

        [Fact]
        public void ShouldDisposeLisetnerSocketIfCancelled()
        {
            // Cancel on first route so we can test socket disposed we will go in infinite loop
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesCancelOnRoute);
            host = newhostBuilder.Build();

            // Start sender and receiver communication and wait for 5 secs
            Task routerTask = host.RunRouterAsSocketServer(listenerSocketInitializerDelegate, SocketFlags.None, new IPEndPoint(IPAddress.Loopback, 12345), 5, tokenSource.Token);
            Task senderTask = SendMessageOverSocket("Test message from sender", 12345);
            bool complete = Task.WaitAll(new Task[] { routerTask, senderTask }, 5000);
            complete.Should().BeTrue();

            // Make sure listener is closed
            EnsureSocketDisposed(listnerSocket);

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            errorPublisher.Called.Should().BeTrue();
            errorPublisher.PublishedMessage.Should().Be("Received cancellation token, the routing is canceled.");
            stringWriter.ToString().Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public void ShouldDisposeLisetnerSocketIfException()
        {
            // Exception is thrown and the routing is canceled
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesExceptionAndCancelOnRoute);
            host = newhostBuilder.Build();

            // Start sender and receiver communication and wait for 5 secs
            Task routerTask = host.RunRouterAsSocketServer(listenerSocketInitializerDelegate, SocketFlags.None, new IPEndPoint(IPAddress.Loopback, 12345), 5, tokenSource.Token);
            Task senderTask = SendMessageOverSocket("Test message from sender", 12345);
            bool complete = Task.WaitAll(new Task[] { routerTask, senderTask }, 5000);
            complete.Should().BeTrue();

            // Make sure listener is closed
            EnsureSocketDisposed(listnerSocket);

            // Check the published error
            MockExceptionPublisher exPublisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            exPublisher.Called.Should().BeTrue();
            exPublisher.PublishedMessage.Should().Be("Test invalid operation.");

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            errorPublisher.Called.Should().BeTrue();
            errorPublisher.PublishedMessage.Should().Be("Received cancellation token, the routing is canceled.");
            stringWriter.ToString().Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public void ShouldDisposeLisetnerSocketIfErrorException()
        {
            // Exception is thrown and the routing is canceled
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesErrorExceptionAndCancelOnRoute);
            host = newhostBuilder.Build();

            // Start sender and receiver communication and wait for 5 secs
            Task routerTask = host.RunRouterAsSocketServer(listenerSocketInitializerDelegate, SocketFlags.None, new IPEndPoint(IPAddress.Loopback, 12345), 5, tokenSource.Token);
            Task senderTask = SendMessageOverSocket("Test message from sender", 12345);
            bool complete = Task.WaitAll(new Task[] { routerTask, senderTask }, 5000);
            complete.Should().BeTrue();

            // Make sure listener is closed
            EnsureSocketDisposed(listnerSocket);

            // Check the published error
            MockExceptionPublisher exPublisher = (MockExceptionPublisher)host.Services.GetRequiredService<IExceptionHandler>();
            exPublisher.Called.Should().BeTrue();
            exPublisher.PublishedMessage.Should().Be("test_error_description. arg1=test1 arg2=test2");

            // Check output
            MockErrorPublisher errorPublisher = (MockErrorPublisher)host.Services.GetRequiredService<IErrorHandler>();
            errorPublisher.Called.Should().BeTrue();
            errorPublisher.PublishedMessage.Should().Be("Received cancellation token, the routing is canceled.");
            stringWriter.ToString().Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public void ShouldNotDisposeLisetnerSocketTillCancelled()
        {
            // Router will go in infinite loop so listener socket will not be disposed.
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = newhostBuilder.Build();

            // Start sender and receiver communication and wait for 5 secs
            Task routerTask = host.RunRouterAsSocketServer(listenerSocketInitializerDelegate, SocketFlags.None, new IPEndPoint(IPAddress.Loopback, 12345), 5);
            Task senderTask = SendMessageOverSocket("Test message from sender", 12345);
            bool complete = Task.WaitAll(new Task[] { routerTask, senderTask }, 2000);
            complete.Should().BeFalse();

            // Verify router called
            MockSocketCommandRouter mockCommandRouter = (MockSocketCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.RawCommandStrings.Should().ContainSingle("Test message from sender");

            // Make sure listener is not closed
            listnerSocket.LocalEndPoint!.ToString().Should().Be("127.0.0.1:12345");
        }

        [Fact]
        public void ReceiveSocketConnectionFromSender()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // Cancel on first route so we can test user input without this we will go in infinite loop
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = newhostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Start sender and receiver communication and wait for 5 secs
            Task routerTask = host.RunRouterAsSocketServer(listenerSocketInitializerDelegate, SocketFlags.None, new IPEndPoint(IPAddress.Loopback, 12345), 5);
            Task senderTask = SendMessageOverSocket("Test message from sender", 12345);
            bool complete = Task.WaitAll(new Task[] { routerTask, senderTask }, 2000);

            // The router will run indefinably
            complete.Should().BeFalse();

            MockSocketCommandRouter mockCommandRouter = (MockSocketCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.RawCommandStrings.Should().ContainSingle("Test message from sender");
        }

        [Fact]
        public void ShouldIgnoreEmptyMessageFromSender()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // Cancel on first route so we can test user input without this we will go in infinite loop
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = newhostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Start sender and receiver communication and wait for 5 secs
            Task routerTask = host.RunRouterAsSocketServer(listenerSocketInitializerDelegate, SocketFlags.None, new IPEndPoint(IPAddress.Loopback, 12345), 5);
            Task senderTask = SendMessageOverSocket("   ", 12345);
            bool complete = Task.WaitAll(new Task[] { routerTask, senderTask }, 2000);

            // The router will run indefinably
            complete.Should().BeFalse();

            MockSocketCommandRouter mockCommandRouter = (MockSocketCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeFalse();
            mockCommandRouter.RawCommandStrings.Should().BeEmpty();
        }

        [Fact]
        public void ReceiveSocketConnectionWithLongMessageFromSender()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // Cancel on first route so we can test user input without this we will go in infinite loop
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = newhostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Start sender and receiver communication and wait for 5 secs
            Task routerTask = host.RunRouterAsSocketServer(listenerSocketInitializerDelegate, SocketFlags.None, new IPEndPoint(IPAddress.Loopback, 12345), 5);
            Task senderTask = SendMessageOverSocket(new string('c', 5000), 12345);
            bool complete = Task.WaitAll(new Task[] { routerTask, senderTask }, 2000);

            // The router will run indefinably
            complete.Should().BeFalse();

            MockSocketCommandRouter mockCommandRouter = (MockSocketCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();
            mockCommandRouter.RawCommandStrings.Should().ContainSingle(new string('c', 5000));
        }

        [Fact]
        public void ReceiveMultipleLongMessagesFromSender()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // Cancel on first route so we can test user input without this we will go in infinite loop
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = newhostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Start sender and receiver communication and wait for 5 secs
            Task routerTask = host.RunRouterAsSocketServer(listenerSocketInitializerDelegate, SocketFlags.None, new IPEndPoint(IPAddress.Loopback, 12345), 5);
            Task senderTask1 = SendMessageOverSocket(new string('c', 5000) + "1", 12345);
            Task senderTask2 = SendMessageOverSocket(new string('c', 5000) + "2", 12345);
            Task senderTask3 = SendMessageOverSocket(new string('c', 5000) + "3", 12345);
            Task senderTask4 = SendMessageOverSocket(new string('c', 5000) + "4", 12345);
            Task senderTask5 = SendMessageOverSocket(new string('c', 5000) + "5", 12345);
            bool complete = Task.WaitAll(new Task[] { routerTask, senderTask1, senderTask2, senderTask3, senderTask4, senderTask5 }, 2000);

            // The router will run indefinably
            complete.Should().BeFalse();

            MockSocketCommandRouter mockCommandRouter = (MockSocketCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();

            // Order is not guaranteed
            mockCommandRouter.RawCommandStrings.Should().BeEquivalentTo(new[] {
                new string('c', 5000) + "1",
                new string('c', 5000) + "2",
                new string('c', 5000) + "4",
                new string('c', 5000) + "5",
                new string('c', 5000) + "3",
            });
        }

        [Fact]
        public void ReceiveMultipleMessagesFromSender()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // Cancel on first route so we can test user input without this we will go in infinite loop
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDefault);
            host = newhostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Start sender and receiver communication and wait for 5 secs
            Task routerTask = host.RunRouterAsSocketServer(listenerSocketInitializerDelegate, SocketFlags.None, new IPEndPoint(IPAddress.Loopback, 12345), 5);
            Task senderTask1 = SendMessageOverSocket("Test message from sender1", 12345);
            Task senderTask2 = SendMessageOverSocket("Test message from sender2", 12345);
            Task senderTask3 = SendMessageOverSocket("Test message from sender3", 12345);
            Task senderTask4 = SendMessageOverSocket("Test message from sender4", 12345);
            Task senderTask5 = SendMessageOverSocket("Test message from sender5", 12345);
            bool complete = Task.WaitAll(new Task[] { routerTask, senderTask1, senderTask2, senderTask3, senderTask4, senderTask5 }, 2000);

            // The router will run indefinably
            complete.Should().BeFalse();

            MockSocketCommandRouter mockCommandRouter = (MockSocketCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();

            // Order is not guaranteed
            mockCommandRouter.RawCommandStrings.Should().BeEquivalentTo(new[] {
                "Test message from sender1",
                "Test message from sender2",
                "Test message from sender4",
                "Test message from sender5",
                "Test message from sender3",
            });
        }

        [Fact]
        public void ShouldIgnoreMessagesOverBacklog()
        {
            // Mock Console read and write
            Console.SetOut(stringWriter);

            // Router adds an explicit delay of 3000 milliseconds
            // Within 3 secs we will fire 5 sender request, with limit set to 3, 2 should be ignored
            var newhostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesDelayAndCancelOnRoute);
            host = newhostBuilder.Build();

            GetCliOptions(host).Router.Timeout = Timeout.Infinite;

            // Send 5 sender commands after 3 seconds, with backlog set to 3 2 requests will be rejected.
            Task routerTask = host.RunRouterAsSocketServer(listenerSocketInitializerDelegate, SocketFlags.None, new IPEndPoint(IPAddress.Loopback, 12345), 3);
            Task senderTask1 = SendMessageOverSocket("Test message from sender1", 12345, 3000);
            Task senderTask2 = SendMessageOverSocket("Test message from sender2", 12345, 3000);
            Task senderTask3 = SendMessageOverSocket("Test message from sender3", 12345, 3000);
            Task senderTask4 = SendMessageOverSocket("Test message from sender4", 12345, 3000);
            Task senderTask5 = SendMessageOverSocket("Test message from sender5", 12345, 3000);

            // Time out of 10 secs so we have enough time for everyone to complete
            bool complete = Task.WaitAll(new Task[] { routerTask, senderTask1, senderTask2, senderTask3, senderTask4, senderTask5 }, 10000);

            // The router will run indefinitely
            complete.Should().BeFalse();

            MockSocketCommandRouter mockCommandRouter = (MockSocketCommandRouter)host.Services.GetRequiredService<ICommandRouter>();
            mockCommandRouter.RouteCalled.Should().BeTrue();

            // Order is not guaranteed
            mockCommandRouter.RawCommandStrings.Should().HaveCount(3);
        }

        private Socket listenerSocketInitializerDelegate()
        {
            listnerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            return listnerSocket;
        }

        private async Task SendMessageOverSocket(string message, int port, int delay = 500)
        {
            // Wait for the listener to initialize
            await Task.Delay(delay);

            using (Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Loopback, port);
                await sender.ConnectAsync(remoteEndPoint);
                await sender.SendAsync(Encoding.UTF8.GetBytes(message));
            }
        }

        private void ConfigureServicesCancelledRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            arg2.AddSingleton<ICommandRouter>(new MockCommandRouterCancellation());
            arg2.AddSingleton(MockCliOptions.New());

            // Tells the logger to write to string writer so we can test it,
            var loggerFactory = new MockLoggerFactory();
            loggerFactory.StringWriter = stringWriter;
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());

            // Add text handler
            arg2.AddSingleton<ITextHandler>(new AsciiTextHandler());
        }

        private void ConfigureServicesCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            arg2.AddSingleton<ICommandRouter>(new MockSocketCommandRouter(null, tokenSource));
            arg2.AddSingleton(MockCliOptions.New());

            // Tells the logger to write to string writer so we can test it,
            var loggerFactory = new MockLoggerFactory();
            loggerFactory.StringWriter = stringWriter;
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());

            // Add text handler
            arg2.AddSingleton<ITextHandler>(new AsciiTextHandler());
        }

        private void ConfigureServicesDefault(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            arg2.AddSingleton<ICommandRouter>(new MockSocketCommandRouter());
            arg2.AddSingleton(MockCliOptions.New());

            // Tells the logger to write to string writer so we can test it,
            var loggerFactory = new MockLoggerFactory();
            loggerFactory.StringWriter = stringWriter;
            arg2.AddSingleton<ILoggerFactory>(new MockLoggerFactory() { StringWriter = stringWriter });

            // Add Error publisher
            arg2.AddSingleton<IErrorHandler>(new MockErrorPublisher());

            // Add text handler
            arg2.AddSingleton<ITextHandler>(new AsciiTextHandler());
        }

        private void ConfigureServicesDelayAndCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();
            arg2.AddSingleton<ICommandRouter>(new MockSocketCommandRouter(3000, tokenSource));
            arg2.AddSingleton(MockCliOptions.New());

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

            // Add text handler
            arg2.AddSingleton<ITextHandler>(new AsciiTextHandler());
        }

        private void ConfigureServicesErrorExceptionAndCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();

            arg2.AddSingleton<ICommandRouter>(new MockCommandRouter(null, tokenSource, new ErrorException("test_error_code", "test_error_description. arg1={0} arg2={1}", "test1", "test2")));
            arg2.AddSingleton(MockCliOptions.New());

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

            // Add text handler
            arg2.AddSingleton<ITextHandler>(new AsciiTextHandler());
        }

        private void ConfigureServicesExceptionAndCancelOnRoute(IServiceCollection arg2)
        {
            tokenSource = new CancellationTokenSource();

            // Adding space at the end so that any msg are correctly appended.
            arg2.AddSingleton<ICommandRouter>(new MockSocketCommandRouter(null, tokenSource, new InvalidOperationException("Test invalid operation.")));
            arg2.AddSingleton(MockCliOptions.New());

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

            // Add text handler
            arg2.AddSingleton<ITextHandler>(new AsciiTextHandler());
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

            // Add text handler
            arg2.AddSingleton<ITextHandler>(new AsciiTextHandler());
        }

        private CliOptions GetCliOptions(IHost host)
        {
            return host.Services.GetRequiredService<CliOptions>();
        }

        private void HostStopRequestCallback(object? state)
        {
            IHost? host = state as IHost;
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(host);
            host.StopAsync().GetAwaiter().GetResult();
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            if (host != null)
            {
                host.Dispose();
            }

            if (stringWriter != null)
            {
                stringWriter.Dispose();
            }

            // Explicitly dispose. This is done by router except for the cases where router goes in infinite loop.
            listnerSocket.Dispose();

            return Task.CompletedTask;
        }

        private void EnsureSocketDisposed(Socket socket)
        {
            try
            {
                int bytes = socket.Available;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            throw new InvalidOperationException($"Socket {socket.LocalEndPoint} should be disposed but it is not.");
        }

        private IHost host = null!;
        private Socket listnerSocket = null!;
        private StringWriter stringWriter = null!;
        private CancellationTokenSource tokenSource = null!;
    }
}