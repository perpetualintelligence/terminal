/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Grpc.Core;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Client.Extensions
{
    public class TerminalGrpcRouterProtoClientExtensionsTests
    {
        public TerminalGrpcRouterProtoClientExtensionsTests()
        {
            grpcClientMock = new Mock<TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient>();
            routeCommandCallCount = 0;

            grpcClientMock.Setup(client => client.RouteCommandAsync(It.IsAny<TerminalGrpcRouterProtoInput>(), null, null, CancellationToken.None))
                .Callback<TerminalGrpcRouterProtoInput, Metadata?, DateTime?, CancellationToken>((request, metadata, dateTime, token) =>
                {
                    capturedRequest = request; // Capture the request for validation
                    routeCommandCallCount++;  // Track the call count
                })
                .Returns(CreateAsyncUnaryCall(new TerminalGrpcRouterProtoOutput())); // Mocked response
        }

        [Fact]
        public async Task PostBatchToTerminalAsync_SendsBatchRequest_WithDelimiters_ReturnsResponse_AndCallsRouteCommandAsyncOnce()
        {
            // Arrange
            var commands = new[] { "command1", "command2", "command3" };
            var cmdDelimiter = ";";
            var msgDelimiter = "|";
            var expectedCommandString = "command1;command2;command3|";

            // Act
            var response = await grpcClientMock.Object.SendBatchToTerminalAsync(commands, cmdDelimiter, msgDelimiter, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();

            // Validate the captured request with FluentAssertions
            capturedRequest.Should().NotBeNull();
            capturedRequest!.CommandString.Should().Be(expectedCommandString); // Using FluentAssertions for validation

            // Ensure that RouteCommandAsync was called exactly once
            routeCommandCallCount.Should().Be(1); // Using FluentAssertions to verify the call count
        }

        [Fact]
        public async Task PostSingleToTerminalAsync_WithDelimiters_SendsRequest_ReturnsResponse_AndCallsRouteCommandAsyncOnce()
        {
            // Arrange
            var command = "test-command";
            var cmdDelimiter = ";";
            var msgDelimiter = "|";
            var expectedCommandString = "test-command|";

            // Act
            var response = await grpcClientMock.Object.SendSingleToTerminalAsync(command, cmdDelimiter, msgDelimiter, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();

            // Validate the captured request with FluentAssertions
            capturedRequest.Should().NotBeNull();
            capturedRequest!.CommandString.Should().Be(expectedCommandString); // Using FluentAssertions for validation

            // Ensure that RouteCommandAsync was called exactly once
            routeCommandCallCount.Should().Be(1); // Using FluentAssertions to verify the call count
        }

        [Fact]
        public async Task PostSingleToTerminalAsync_WithoutDelimiters_SendsRequest_ReturnsResponse_AndCallsRouteCommandAsyncOnce()
        {
            // Arrange
            var command = "test-command";
            var expectedCommandString = "test-command";

            // Act
            var response = await grpcClientMock.Object.SendSingleToTerminalAsync(command, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();

            // Validate the captured request with FluentAssertions
            capturedRequest.Should().NotBeNull();
            capturedRequest!.CommandString.Should().Be(expectedCommandString); // Using FluentAssertions for validation

            // Ensure that RouteCommandAsync was called exactly once
            routeCommandCallCount.Should().Be(1); // Using FluentAssertions to verify the call count
        }

        private static AsyncUnaryCall<TResponse> CreateAsyncUnaryCall<TResponse>(TResponse response)
        {
            var responseTask = Task.FromResult(response);
            var metadataTask = Task.FromResult(new Metadata());

            return new AsyncUnaryCall<TResponse>(
                responseTask,                       // The task that returns the response.
                metadataTask,                       // The task that returns the response headers (Metadata).
                () => Status.DefaultSuccess,        // Function that returns the call status.
                () => [],                           // Function that returns the trailers (Metadata).
                () => { }                           // Delegate to handle call dispose.
                                                );
        }

        private readonly Mock<TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient> grpcClientMock;
        private TerminalGrpcRouterProtoInput? capturedRequest; // Captured request for FluentAssertions validation
        private int routeCommandCallCount; // To track number of calls
    }
}
