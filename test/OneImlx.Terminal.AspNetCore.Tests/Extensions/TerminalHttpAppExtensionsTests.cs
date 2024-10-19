/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OneImlx.Terminal.AspNetCore.Extensions;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.AspNetCore.Tests
{
    public class TerminalHttpAppExtensionsTests
    {
        public TerminalHttpAppExtensionsTests()
        {
            // Mocking the dependencies to isolate the actual queue and terminal router behavior
            _mockCommandRouter = new Mock<ICommandRouter>();
            _mockTerminalRouter = new Mock<ITerminalRouter<TerminalHttpRouterContext>>();
            _mockExceptionHandler = new Mock<ITerminalExceptionHandler>();
            _mockQueueLogger = new Mock<ILogger<TerminalQueue>>();
            _mockTerminalServiceLogger = new Mock<ILogger<TerminalHttpMapService>>();
            _terminalOptions = new TerminalOptions();

            // The terminal router context is mocked here because we are not testing its internal behavior but need to
            // simulate its presence for the terminal service to function properly.
            _terminalRouterContext = new TerminalHttpRouterContext(
                new IPEndPoint(IPAddress.Loopback, 8000),
                new TerminalStartContext(TerminalStartMode.Http, CancellationToken.None, CancellationToken.None, null));
        }

        [Fact]
        public async Task MapTerminalHttp_PostCommand_QueueShouldHaveOneItem()
        {
            // We are using a real instance of TerminalRemoteMessageQueue instead of mocking it, because we want to
            // verify that the queue's state changes after a POST request.
            var terminalQueue = new TerminalQueue(
                _mockCommandRouter.Object,
                _mockExceptionHandler.Object,
                _terminalOptions,
                _terminalRouterContext,
                _mockQueueLogger.Object);

            // Critical step: Setting up the terminal router to use the real queue. The queue is integral to this test,
            // and we need the mocked terminal router to point to this real queue so that it can interact with it during
            // the POST request.
            _mockTerminalRouter.Setup(r => r.CommandQueue).Returns(terminalQueue);

            // The terminalMapService is responsible for processing HTTP requests. We pass the mocked terminal router
            // and the logger. This is the component we are indirectly testing by making an HTTP POST call.
            var terminalMapService = new TerminalHttpMapService(
                _mockTerminalRouter.Object,
                _mockTerminalServiceLogger.Object);

            // Setting up an in-memory TestServer to simulate an actual ASP.NET Core environment where the HTTP POST
            // request will be made to the `/oneimlx/terminal/httprouter` endpoint.
            // Critical: We inject the real `terminalMapService` into the test server, which ensures the service processes
            // the incoming HTTP requests and interacts with the real queue.
            var server = new TestServer(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddSingleton(terminalMapService);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        // Map the POST endpoint we are testing
                        endpoints.MapTerminalHttp();
                    });
                }));

            var client = server.CreateClient();

            // Pre-condition check: Ensure that the queue starts empty. This is critical because we are testing that the
            // POST request triggers the queue to enqueue a command.
            terminalQueue.RequestCount.Should().Be(0, "the queue should start empty before the POST request");

            // Act: Sending a POST request to the `/oneimlx/terminal/httprouter` endpoint with a sample command.
            var jsonRequestBody = JsonSerializer.Serialize(new TerminalJsonCommandRequest("test_command"));
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/oneimlx/terminal/httprouter", content);
            response.EnsureSuccessStatusCode(); // Ensure that the request was successful (HTTP 200)

            // Post-condition check: After the POST request, there should be exactly one item in the queue. This
            // validates that the request correctly triggered the command to be enqueued in the queue.
            terminalQueue.RequestCount.Should().Be(1, "one command should be enqueued after processing the POST request");
        }

        private readonly Mock<ICommandRouter> _mockCommandRouter;
        private readonly Mock<ITerminalExceptionHandler> _mockExceptionHandler;
        private readonly Mock<ILogger<TerminalQueue>> _mockQueueLogger;
        private readonly Mock<ITerminalRouter<TerminalHttpRouterContext>> _mockTerminalRouter;
        private readonly Mock<ILogger<TerminalHttpMapService>> _mockTerminalServiceLogger;
        private readonly TerminalOptions _terminalOptions;
        private readonly TerminalRouterContext _terminalRouterContext;
    }
}
