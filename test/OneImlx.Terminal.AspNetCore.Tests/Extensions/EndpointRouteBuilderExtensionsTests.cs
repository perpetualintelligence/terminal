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
using OneImlx.Terminal.Server.Extensions;
using OneImlx.Terminal.Runtime;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Server.Tests
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
            TerminalInput terminalInput = TerminalInput.Single("test-id", "test-command");
            TerminalOutput terminalOutput = new(terminalInput, ["any"], null, null);
            bool executeCalled = false;
            mockTerminalRouter.Setup(x => x.IsRunning).Returns(true);
            mockTerminalProcessor.Setup(x => x.IsProcessing).Returns(true);
            mockTerminalProcessor.Setup(x => x.ExecuteAsync(It.IsAny<TerminalInput>(), It.IsAny<string>(), It.IsAny<string>()))
                                 .Callback(() => executeCalled = true)
                                 .ReturnsAsync(terminalOutput);

            // Act: Post to /oneimlx/terminal/httprouter
            var response = await client.PostAsJsonAsync("/oneimlx/terminal/httprouter", terminalInput);

            // Assert: Ensure TerminalHttpMapService's RouteCommandAsync method is invoked correctly
            response.EnsureSuccessStatusCode();
            executeCalled.Should().BeTrue();
            TerminalOutput? outputFromResponse = await response.Content.ReadFromJsonAsync<TerminalOutput>();
            outputFromResponse.Should().NotBeNull();
            outputFromResponse.Should().NotBeSameAs(terminalOutput);
            outputFromResponse!.Input.Should().NotBeSameAs(terminalOutput.Input);

            outputFromResponse!.Input.Requests[0].Id.Should().Be("test-id");
            outputFromResponse!.Input.Requests[0].Raw.Should().Be("test-command");
            outputFromResponse.GetDeserializedResult<string>(0).Should().Be("any");
            outputFromResponse.SenderId.Should().BeNull();
            outputFromResponse.SenderEndpoint.Should().BeNull();
        }
    }
}
