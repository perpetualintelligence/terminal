/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Moq;
using OneImlx.Terminal.Runtime;
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
        public async Task SendToTerminalAsync_Sends_Input_As_Batch_Correctly()
        {
            // Arrange
            var cmdIds = new[] { "id1", "id2", "id3" };
            var commands = new[] { "command1", "command2", "command3" };

            // Act
            TerminalInput input = TerminalInput.Batch("batch1", cmdIds, commands);
            var response = await grpcClientMock.Object.SendToTerminalAsync(input, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();

            // Validate the captured request with FluentAssertions
            capturedRequest.Should().NotBeNull();
            capturedRequest!.InputJson.Should().Be("{\"batch_id\":\"batch1\",\"requests\":[{\"id\":\"id1\",\"is_error\":false,\"raw\":\"command1\",\"result\":null},{\"id\":\"id2\",\"is_error\":false,\"raw\":\"command2\",\"result\":null},{\"id\":\"id3\",\"is_error\":false,\"raw\":\"command3\",\"result\":null}]}");

            // Ensure that RouteCommandAsync was called exactly once
            routeCommandCallCount.Should().Be(1); // Using FluentAssertions to verify the call count
        }

        [Fact]
        public async Task SendToTerminalAsync_Sends_Input_As_Single_Correctly()
        {
            // Act
            TerminalInput input = TerminalInput.Single("id1", raw: "test-command");
            var response = await grpcClientMock.Object.SendToTerminalAsync(input, CancellationToken.None);

            // Assert
            response.Should().NotBeNull();

            // Validate the captured request with FluentAssertions
            capturedRequest.Should().NotBeNull();
            capturedRequest!.InputJson.Should().Be("{\"batch_id\":null,\"requests\":[{\"id\":\"id1\",\"is_error\":false,\"raw\":\"test-command\",\"result\":null}]}");

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
                static () => Status.DefaultSuccess,        // Function that returns the call status.
                static () => [],                           // Function that returns the trailers (Metadata).
                static () => { }                           // Delegate to handle call dispose.
                                                );
        }

        private readonly Mock<TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient> grpcClientMock;
        private TerminalGrpcRouterProtoInput? capturedRequest; // Captured request for FluentAssertions validation
        private int routeCommandCallCount; // To track number of calls
    }
}
