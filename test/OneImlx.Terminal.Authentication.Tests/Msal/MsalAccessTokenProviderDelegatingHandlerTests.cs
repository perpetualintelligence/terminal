/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions.Authentication;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Authentication.Msal
{
    public class MsalAccessTokenProviderDelegatingHandlerTests
    {
        private readonly Mock<IAccessTokenProvider> _mockAccessTokenProvider;
        private readonly Mock<ILogger<MsalAccessTokenProviderDelegatingHandler>> _mockLogger;
        private readonly MsalAccessTokenProviderDelegatingHandler _handler;

        public MsalAccessTokenProviderDelegatingHandlerTests()
        {
            _mockAccessTokenProvider = new Mock<IAccessTokenProvider>();
            _mockLogger = new Mock<ILogger<MsalAccessTokenProviderDelegatingHandler>>();
            _handler = new MsalAccessTokenProviderDelegatingHandler(_mockAccessTokenProvider.Object, _mockLogger.Object)
            {
                InnerHandler = new TestHandler() // A dummy handler to complete the pipeline
            };
        }

        [Fact]
        public async Task SendAsync_ShouldCallBaseSendAsync()
        {
            // Arrange
            var testHandler = new TestHandler();
            _mockAccessTokenProvider.Setup(x => x.GetAuthorizationTokenAsync(It.IsAny<Uri>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                                    .ReturnsAsync("mock_token");
            var delegatingHandler = new MsalAccessTokenProviderDelegatingHandler(_mockAccessTokenProvider.Object, _mockLogger.Object)
            {
                InnerHandler = testHandler
            };
            var invoker = new HttpMessageInvoker(delegatingHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            await invoker.SendAsync(request, new CancellationToken());

            // Assert
            testHandler.SendAsyncCalled.Should().BeTrue();
        }

        [Fact]
        public async Task SendAsync_ShouldPassHttpRequestOptions()
        {
            // Arrange
            var expectedToken = "access_token";
            Dictionary<string, object>? capturedProperties = null;

            _mockAccessTokenProvider.Setup(x => x.GetAuthorizationTokenAsync(It.IsAny<Uri>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                                    .Callback<Uri, Dictionary<string, object>, CancellationToken>((uri, props, token) => capturedProperties = props)
                                    .ReturnsAsync(expectedToken);

            var invoker = new HttpMessageInvoker(_handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            request.Options.TryAdd("key1", "value1");
            request.Options.TryAdd("key2", 23);

            // Act
            await invoker.SendAsync(request, new CancellationToken());

            // Assert
            capturedProperties.Should().NotBeNull();
            capturedProperties.Should().ContainKey("key1").WhoseValue.Should().Be("value1");
            capturedProperties.Should().ContainKey("key2").WhoseValue.Should().Be(23);
        }

        [Fact]
        public async Task SendAsync_ShouldAddAccessTokenToRequest()
        {
            // Arrange
            var expectedToken = "access_token";
            _mockAccessTokenProvider.Setup(x => x.GetAuthorizationTokenAsync(It.IsAny<Uri>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(expectedToken);

            var invoker = new HttpMessageInvoker(_handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            var response = await invoker.SendAsync(request, new CancellationToken());

            // Assert
            _mockAccessTokenProvider.Verify(x => x.GetAuthorizationTokenAsync(It.IsAny<Uri>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
            request.Headers.Authorization.Should().BeEquivalentTo(new AuthenticationHeaderValue("Bearer", expectedToken));
        }

        [Fact]
        public async Task SendAsync_ShouldThrowException_WhenTokenAcquisitionFails()
        {
            // Arrange
            _mockAccessTokenProvider.Setup(x => x.GetAuthorizationTokenAsync(It.IsAny<Uri>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                                    .ThrowsAsync(new Exception("Token acquisition failed"));

            var invoker = new HttpMessageInvoker(_handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act & Assert
            Func<Task> func = () => invoker.SendAsync(request, new CancellationToken());
            await func.Should().ThrowAsync<Exception>().WithMessage("Token acquisition failed");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("     ")]
        public async Task SendAsync_ShouldThrowException_WhenTokenIsNullOrWhitespace(string? token)
        {
            // Arrange
            _mockAccessTokenProvider.Setup(x => x.GetAuthorizationTokenAsync(It.IsAny<Uri>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(token!);

            var invoker = new HttpMessageInvoker(_handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act & Assert
            Func<Task> func = () => invoker.SendAsync(request, new CancellationToken());
            await func.Should().ThrowAsync<TerminalException>().WithMessage("The access token is null or empty.");
        }

        [Fact]
        public async Task SendAsync_ShouldCallPreflightAsync()
        {
            // Arrange
            var testHandler = new TestMsalAccessTokenProviderDelegatingHandler(_mockAccessTokenProvider.Object, _mockLogger.Object)
            {
                InnerHandler = new TestHandler() // Use the existing TestHandler
            };

            testHandler.PreflightAsyncCalled.Should().BeFalse();

            _mockAccessTokenProvider.Setup(x => x.GetAuthorizationTokenAsync(It.IsAny<Uri>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                                    .ReturnsAsync("mock_token");

            var invoker = new HttpMessageInvoker(testHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            await invoker.SendAsync(request, new CancellationToken());

            // Assert
            testHandler.PreflightAsyncCalled.Should().BeTrue();
        }
    }
}