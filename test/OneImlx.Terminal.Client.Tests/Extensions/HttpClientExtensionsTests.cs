/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Moq;
using Moq.Protected;
using OneImlx.Terminal.Runtime;
using System.Net;
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
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new System.Uri("http://localhost")
            };
        }

        [Fact]
        public async Task PostBatchToTerminalAsync_SendsBatchRequest_WithDelimiters_ReturnsResponse()
        {
            // Arrange
            var commands = new[] { "command1", "command2", "command3" };
            var cmdDelimiter = ";";
            var msgDelimiter = "|";

            // Act
            var response = await _httpClient.SendBatchAsync(commands, cmdDelimiter, msgDelimiter, CancellationToken.None);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify that the HTTP request content was correct
            TerminalJsonRequest? actualContent = await _capturedRequest!.Content!.ReadFromJsonAsync<TerminalJsonRequest>();
            actualContent.Should().NotBeNull();
            actualContent!.Raw.Should().Be("command1;command2;command3|");
        }

        [Fact]
        public async Task PostSingleToTerminalAsync_WithDelimiters_SendsRequest_ReturnsResponse()
        {
            // Arrange
            var command = "test-command";
            var cmdDelimiter = ";";
            var msgDelimiter = "|";

            // Act
            var response = await _httpClient.SendSingleAsync(command, cmdDelimiter, msgDelimiter, CancellationToken.None);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify that the HTTP request content was correct
            TerminalJsonRequest? actualContent = await _capturedRequest!.Content!.ReadFromJsonAsync<TerminalJsonRequest>();
            actualContent.Should().NotBeNull();
            actualContent!.Raw.Should().Be("test-command|");
        }

        [Fact]
        public async Task PostSingleToTerminalAsync_WithoutDelimiters_SendsRequest_ReturnsResponse()
        {
            // Arrange
            var command = "test-command";

            // Act
            var response = await _httpClient.SendSingleAsync(command, CancellationToken.None);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify that the HTTP request content was correct
            TerminalJsonRequest? actualContent = await _capturedRequest!.Content!.ReadFromJsonAsync<TerminalJsonRequest>();
            actualContent.Should().NotBeNull();
            actualContent!.Raw.Should().Be("test-command");
        }

        private readonly HttpClient _httpClient;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpRequestMessage? _capturedRequest; // Capture the request for content verification
    }
}
