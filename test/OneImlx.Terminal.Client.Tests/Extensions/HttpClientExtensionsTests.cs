/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Moq;
using Moq.Protected;
using OneImlx.Terminal.Runtime;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Client.Extensions
{
    public class HttpClientExtensionsTests
    {
        public HttpClientExtensionsTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
                {
                    _capturedRequest = req; // Capture the request for verification
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    // Any response we do not validate response here only the request.
                    Content = JsonContent.Create(new TerminalOutput(TerminalInput.Batch("any_bid", [], []), null, null))
                });

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
        }

        [Fact]
        public async Task SendToTerminal_SendsBatch_Correctly()
        {
            // Arrange
            var cmdIds = new[] { "cmd1", "cmd2", "cmd3" };
            var commands = new[] { "command1", "command2", "command3" };

            // Act
            TerminalInput input = TerminalInput.Batch(Guid.NewGuid().ToString(), cmdIds, commands);
            TerminalOutput? output = await _httpClient.SendToTerminalAsync(input, CancellationToken.None);

            // Verify that the HTTP request content was correct
            _capturedRequest!.RequestUri.Should().Be(new Uri("http://localhost/oneimlx/terminal/httprouter"));
            TerminalInput? actualContent = await _capturedRequest.Content!.ReadFromJsonAsync<TerminalInput>();
            actualContent.Should().NotBeNull();
            actualContent!.Count.Should().Be(3);

            // By Index (Maintaining the order)
            actualContent[0].Id.Should().Be("cmd1");
            actualContent[0].Raw.Should().Be("command1");
            actualContent[1].Id.Should().Be("cmd2");
            actualContent[1].Raw.Should().Be("command2");
            actualContent[2].Id.Should().Be("cmd3");
            actualContent[2].Raw.Should().Be("command3");

            actualContent.IsBatch.Should().BeTrue();

            _capturedRequest.Method.Should().Be(HttpMethod.Post);
        }

        [Fact]
        public async Task SendToTerminal_SendsRequest_Correctly()
        {
            // Arrange
            var command = "test-command";

            // Act
            TerminalInput input = TerminalInput.Single("cmd1", command);
            TerminalOutput? output = await _httpClient.SendToTerminalAsync(input, CancellationToken.None);

            // Verify that the HTTP request content was correct
            _capturedRequest!.RequestUri.Should().Be(new Uri("http://localhost/oneimlx/terminal/httprouter"));
            TerminalInput? actualContent = await _capturedRequest.Content!.ReadFromJsonAsync<TerminalInput>();
            actualContent.Should().NotBeNull();
            actualContent!.Count.Should().Be(1);
            actualContent[0].Id.Should().Be("cmd1");
            actualContent[0].Raw.Should().Be("test-command");

            actualContent.IsBatch.Should().BeFalse();

            _capturedRequest.Method.Should().Be(HttpMethod.Post);
        }

        private readonly HttpClient _httpClient;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpRequestMessage? _capturedRequest; // Capture the request for content verification
    }
}
