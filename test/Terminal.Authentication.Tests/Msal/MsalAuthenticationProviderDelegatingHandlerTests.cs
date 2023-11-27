/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Authentication.Msal
{
    public class MsalAuthenticationProviderDelegatingHandlerTests
    {
        private readonly Mock<IAuthenticationProvider> _mockAuthenticationProvider;
        private readonly Mock<ILogger<MsalAuthenticationProviderDelegatingHandler>> _mockLogger;
        private readonly MsalAuthenticationProviderDelegatingHandler _handler;

        public MsalAuthenticationProviderDelegatingHandlerTests()
        {
            _mockAuthenticationProvider = new Mock<IAuthenticationProvider>();
            _mockLogger = new Mock<ILogger<MsalAuthenticationProviderDelegatingHandler>>();
            _handler = new MsalAuthenticationProviderDelegatingHandler(_mockAuthenticationProvider.Object, _mockLogger.Object)
            {
                InnerHandler = new TestHandler() // A dummy handler to complete the pipeline
            };
        }

        [Fact]
        public async Task SendAsync_ShouldPassHttpRequestOptions()
        {
            // Arrange
            Dictionary<string, object>? capturedProperties = null;
            _mockAuthenticationProvider.Setup(p => p.AuthenticateRequestAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .Callback<RequestInformation, Dictionary<string, object>, CancellationToken>((reqInfo, props, token) =>
                {
                    capturedProperties = props;
                    reqInfo.Headers["Authorization"] = new List<string>() { "Bearer mock_token" }; // Add the authorization header
                })
                .Returns(Task.CompletedTask);

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
        public async Task SendAsync_ShouldCallBaseSendAsync()
        {
            // Arrange
            var testHandler = new TestHandler();
            _mockAuthenticationProvider.Setup(p => p.AuthenticateRequestAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                                       .Callback<RequestInformation, Dictionary<string, object>, CancellationToken>((reqInfo, dict, token) =>
                                       {
                                           reqInfo.Headers["Authorization"] = new List<string>() { "Bearer mock_token" }; // Add the authorization header
                                       })
                                       .Returns(Task.CompletedTask);
            var delegatingHandler = new MsalAuthenticationProviderDelegatingHandler(_mockAuthenticationProvider.Object, _mockLogger.Object)
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
        public async Task SendAsync_ShouldAuthenticateRequest()
        {
            // Arrange
            var requestInfoCaptured = new List<RequestInformation>();
            _mockAuthenticationProvider.Setup(p => p.AuthenticateRequestAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .Callback<RequestInformation, Dictionary<string, object>, CancellationToken>((reqInfo, dict, token) =>
                {
                    reqInfo.Headers["Authorization"] = new List<string>() { "Bearer mock_token" }; // Add the authorization header
                    requestInfoCaptured.Add(reqInfo);
                })
                .Returns(Task.CompletedTask);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            var invoker = new HttpMessageInvoker(_handler);
            await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            requestInfoCaptured.Should().ContainSingle();
            requestInfoCaptured[0].URI.Should().Be("https://example.com");
            request.Headers.Authorization.Should().NotBeNull(); // Ensure the "Authorization" header is present
        }

        [Fact]
        public async Task SendAsync_ShouldThrowException_OnAuthenticationFailure()
        {
            // Arrange
            var expectedException = new AuthenticationException("Authentication failed");
            _mockAuthenticationProvider.Setup(p => p.AuthenticateRequestAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                                       .ThrowsAsync(expectedException);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var invoker = new HttpMessageInvoker(_handler);

            // Act & Assert
            Func<Task> act = () => invoker.SendAsync(request, CancellationToken.None);

            // Fluent Assertions method to assert an exception is thrown
            await act.Should().ThrowAsync<AuthenticationException>()
                .WithMessage("Authentication failed");
        }

        [Fact]
        public async Task SendAsync_ShouldAttachTokenFromAuthenticationProvider()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MsalAuthenticationProviderDelegatingHandler>>();
            _mockAuthenticationProvider.Setup(p => p.AuthenticateRequestAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                                       .Callback<RequestInformation, Dictionary<string, object>, CancellationToken>((reqInfo, dict, token) =>
                                       {
                                           // Add the authorization header
                                           reqInfo.Headers["Authorization"] = new List<string>() { "Bearer mock_token" };
                                       })
                                       .Returns(Task.CompletedTask);

            var invoker = new HttpMessageInvoker(_handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            await invoker.SendAsync(request, new CancellationToken());

            // Assert
            request.Headers.Authorization.Should().NotBeNull();
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.Authorization.Parameter.Should().Be("Bearer mock_token");
        }

        [Fact]
        public async Task SendAsync_ShouldThrowException_WhenAuthorizationHeaderIsMissing()
        {
            // Arrange
            _mockAuthenticationProvider.Setup(p => p.AuthenticateRequestAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .Callback<RequestInformation, Dictionary<string, object>, CancellationToken>((reqInfo, dict, token) => { /* Do not add authorization header */ })
                .Returns(Task.CompletedTask);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act & Assert
            var invoker = new HttpMessageInvoker(_handler);
            Func<Task> act = () => invoker.SendAsync(request, CancellationToken.None);

            // Fluent Assertions method to assert an exception is thrown
            await act.Should().ThrowAsync<TerminalException>()
                .WithMessage("The authentication provider missed an authorization header.*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("     ")]
        public async Task SendAsync_ShouldThrowException_WhenTokenIsNullOrEmpty(string? token)
        {
            // Arrange
            _mockAuthenticationProvider.Setup(p => p.AuthenticateRequestAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .Callback<RequestInformation, Dictionary<string, object>, CancellationToken>((reqInfo, dict, cToken) =>
                {
                    reqInfo.Headers["Authorization"] = new List<string>() { token! }; // Empty authorization header
                })
                .Returns(Task.CompletedTask);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act & Assert
            var invoker = new HttpMessageInvoker(_handler);
            Func<Task> act = () => invoker.SendAsync(request, CancellationToken.None);

            // Fluent Assertions method to assert an exception is thrown
            await act.Should().ThrowAsync<TerminalException>()
                .WithMessage("The access_token is null or empty.");
        }
    }
}