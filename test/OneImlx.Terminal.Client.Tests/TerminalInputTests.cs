/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using System;
using System.Text.Json;
using Xunit;

namespace OneImlx.Terminal.Runtime.Tests
{
    public class TerminalInputTests
    {
        [Fact]
        public void Batch_CreatesTerminalInputWithMultipleRequests()
        {
            // Arrange
            var ids = new[] { "id1", "id2" };
            var raws = new[] { "command1", "command2" };

            // Act
            var terminalInput = TerminalInputOutput.Batch("batch1", ids, raws);

            // Assert
            terminalInput.Count.Should().Be(2);

            terminalInput.IsBatch.Should().BeTrue();
            terminalInput.BatchId.Should().Be("batch1");

            terminalInput.Requests.Should().HaveCount(2);
            terminalInput.Requests[0].Id.Should().Be("id1");
            terminalInput.Requests[0].Raw.Should().Be("command1");
            terminalInput.Requests[1].Id.Should().Be("id2");
            terminalInput.Requests[1].Raw.Should().Be("command2");
        }

        [Fact]
        public void Batch_ThrowsArgumentException_WhenIdsAndRawsLengthMismatch()
        {
            // Arrange
            var ids = new[] { "id1", "id2" };
            var raws = new[] { "command1" };

            // Act
            Action act = () => TerminalInputOutput.Batch("batch1", ids, raws);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("The number of command IDs must match the number of raw commands.");
        }

        [Fact]
        public void Batch_ThrowsArgumentException_WhenRequestsArrayIsEmpty()
        {
            // Act
            Action act = () => TerminalInputOutput.Batch("batch1", []);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("The number of requests must be greater than zero.");
        }

        [Fact]
        public void Constructor_InitializesEmptyRequests()
        {
            // Act
            var terminalInput = new TerminalInputOutput();

            // Assert
            terminalInput.Count.Should().Be(0);
            terminalInput.IsBatch.Should().BeFalse();
            terminalInput.BatchId.Should().BeNull();
            terminalInput.Requests.Should().BeEmpty();
        }

        [Fact]
        public void Indexer_GetsCorrectRequest()
        {
            // Arrange
            var terminalInput = TerminalInputOutput.Single("id1", "command1");

            // Act
            var request = terminalInput[0];

            // Assert
            request.Id.Should().Be("id1");
            request.Raw.Should().Be("command1");
        }

        [Fact]
        public void Indexer_SetsCorrectRequest()
        {
            // Arrange
            var terminalInput = TerminalInputOutput.Single("id1", "command1");
            var newRequest = new TerminalRequest("id2", "command2");

            // Act
            terminalInput[0] = newRequest;

            // Assert
            terminalInput[0].Id.Should().Be("id2");
            terminalInput[0].Raw.Should().Be("command2");
        }

        [Fact]
        public void JsonDeserialization_DeSerializesCorrectly()
        {
            // Arrange
            var json = "{\"batch_id\":\"batch1\",\"requests\":[{\"id\":\"id1\",\"raw\":\"command1\"},{\"id\":\"id2\",\"raw\":\"command2\"}],\"sender_endpoint\":\"endpoint1\",\"sender_id\":\"sender1\"}";

            // Act
            var terminalInput = JsonSerializer.Deserialize<TerminalInputOutput>(json);

            // Assert
            terminalInput.Should().NotBeNull();
            terminalInput!.BatchId.Should().Be("batch1");
            terminalInput.Count.Should().Be(2);
            terminalInput.Requests[0].Id.Should().Be("id1");
            terminalInput.Requests[0].Raw.Should().Be("command1");
            terminalInput.Requests[1].Id.Should().Be("id2");
            terminalInput.Requests[1].Raw.Should().Be("command2");
        }

        [Fact]
        public void JsonSerialization_SerializesCorrectly()
        {
            var terminalInput = TerminalInputOutput.Batch("batch1",
            [
                new TerminalRequest("id1", "command1"),
                new TerminalRequest("id2", "command2")
            ]);

            var json = JsonSerializer.Serialize(terminalInput);
            json.Should().Be("{\"batch_id\":\"batch1\",\"requests\":[{\"id\":\"id1\",\"is_error\":false,\"raw\":\"command1\",\"result\":null},{\"id\":\"id2\",\"is_error\":false,\"raw\":\"command2\",\"result\":null}],\"sender_endpoint\":null,\"sender_id\":null}");
        }

        [Fact]
        public void Single_CreatesTerminalInputWithOneRequest()
        {
            // Act
            var terminalInput = TerminalInputOutput.Single("id1", "command1");

            // Assert
            terminalInput.Count.Should().Be(1);
            terminalInput.Requests[0].Id.Should().Be("id1");
            terminalInput.Requests[0].Raw.Should().Be("command1");

            terminalInput.IsBatch.Should().BeFalse();
            terminalInput.BatchId.Should().BeNull();
        }
    }
}
