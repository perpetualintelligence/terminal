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
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Server.Extensions;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Server
{
    public class EndpointRouteBuilderExtensionsTests
    {
        [Fact]
        public async Task Post_To_TerminalHttpRouter_Should_Invoke_TerminalHttpMapService()
        {
            // Arrange
            IPEndPoint iPEndPoint = new(IPAddress.Loopback, 12345);
            var mockTerminalRouter = new Mock<ITerminalRouter<TerminalHttpRouterContext>>();
            var mockTerminalProcessor = new Mock<ITerminalProcessor>();
            var mockLogger = new Mock<ILogger<TerminalHttpMapService>>();
            var terminalRouterContext = new TerminalHttpRouterContext(iPEndPoint, TerminalStartMode.Http, default, default, null);

            // Create a TestServer and configure services and pipeline in the test itself
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddRouting(); // Required for endpoint routing
                    services.AddSingleton(new TerminalHttpMapService(mockTerminalRouter.Object, mockTerminalProcessor.Object, mockLogger.Object));
                })
                .Configure(app =>
                {
                    // Use routing and map the terminal HTTP endpoint
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapTerminalHttp());
                });

            var server = new TestServer(builder);
            var client = server.CreateClient();

            // Setup router and processor to return true for IsRunning and IsProcessing
            TerminalInputOutput terminalInput = TerminalInputOutput.Single("test-id", "test-command");
            bool executeCalled = false;
            mockTerminalRouter.Setup(x => x.IsRunning).Returns(true);
            mockTerminalProcessor.Setup(x => x.IsProcessing).Returns(true);
            mockTerminalProcessor.Setup(x => x.ExecuteAsync(It.IsAny<TerminalInputOutput>()))
                                 .Callback(() => executeCalled = true);

            // Act: Post to /oneimlx/terminal/httprouter
            var response = await client.PostAsJsonAsync("/oneimlx/terminal/httprouter", terminalInput);

            // Assert: Ensure TerminalHttpMapService's RouteCommandAsync method is invoked correctly
            response.EnsureSuccessStatusCode();
            executeCalled.Should().BeTrue();
            TerminalInputOutput? outputFromResponse = await response.Content.ReadFromJsonAsync<TerminalInputOutput>();
            outputFromResponse.Should().NotBeNull();
            outputFromResponse.Should().NotBeSameAs(terminalInput);

            outputFromResponse!.Requests[0].Id.Should().Be("test-id");
            outputFromResponse.Requests[0].Raw.Should().Be("test-command");
            outputFromResponse.Requests[0].Result.Should().BeNull();
            outputFromResponse.SenderId.Should().BeNull();
            outputFromResponse.SenderEndpoint.Should().BeNull();
        }
    }
}
