/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Terminal.Runtime;
using System;
using System.Text.Json;
using Xunit;

namespace OneImlx.Terminal.Tests.Runtime
{
    public class TerminalBatchTests
    {
        [Fact]
        public void Add_Should_Add_Multiple_Commands()
        {
            var batch = new TerminalBatch("test_batch");
            var ids = new[] { "cmd1", "cmd2" };
            var raws = new[] { "raw_command1", "raw_command2" };

            batch.Add(ids, raws);

            batch.Count.Should().Be(2);
            batch["cmd1"].Raw.Should().Be("raw_command1");
            batch["cmd2"].Raw.Should().Be("raw_command2");
        }

        [Fact]
        public void Add_Should_Add_Single_Command()
        {
            var batch = new TerminalBatch("test_batch")
            {
                { "cmd1", "raw_command1" }
            };

            batch.Count.Should().Be(1);
            batch["cmd1"].Raw.Should().Be("raw_command1");
        }

        [Fact]
        public void Add_Should_Throw_ArgumentException_When_Ids_And_Raws_Length_Do_Not_Match()
        {
            var batch = new TerminalBatch("test_batch");
            var ids = new[] { "cmd1" };
            var raws = new[] { "raw_command1", "raw_command2" };

            Action act = () => batch.Add(ids, raws);
            act.Should().Throw<ArgumentException>().WithMessage("The number of command ids must match the number of commands.");
        }

        [Fact]
        public void Constructor_Should_Set_BatchId()
        {
            var batchId = "test_batch";
            var batch = new TerminalBatch(batchId);
            batch.BatchId.Should().Be(batchId);
        }

        [Fact]
        public void Constructor_Should_Throw_ArgumentNullException_When_BatchId_Is_Null()
        {
            Action act = static () => new TerminalBatch(null);
            act.Should().Throw<ArgumentNullException>().WithMessage("The batch id cannot be null. (Parameter 'batchId')");
        }

        [Fact]
        public void JsonIsCorrect()
        {
            // Verify the JSON serialization and deserialization
            var batch = new TerminalBatch("test_batch");
            batch.Add("cmd1", "raw_command1");
            batch.Add("cmd2", "raw_command2");
            batch.Add("cmd3", "raw_command3");
            batch.Add("cmd4", "raw_command4");
            batch.Add("cmd5", "raw_command5");

            var json = JsonSerializer.Serialize(batch);

            var newBatch = JsonSerializer.Deserialize<TerminalBatch>(json);
            newBatch.Should().NotBeSameAs(batch);
            newBatch.Should().BeEquivalentTo(batch);
        }

        [Fact]
        public void Query_Is_In_Order()
        {
            var batch = new TerminalBatch("test_batch");
            var command1 = new TerminalCommand("cmd1", "raw_command1");
            var command2 = new TerminalCommand("cmd2", "raw_command2");
            var command3 = new TerminalCommand("cmd3", "raw_command3");
            var command4 = new TerminalCommand("cmd4", "raw_command4");
            var command5 = new TerminalCommand("cmd5", "raw_command5");

            batch.Add(command1);
            batch.Add(command2);
            batch.Add(command3);
            batch.Add(command4);
            batch.Add(command5);

            // Verify ordered collection
            batch[0].Id.Should().Be("cmd1");
            batch[1].Id.Should().Be("cmd2");
            batch[2].Id.Should().Be("cmd3");
            batch[3].Id.Should().Be("cmd4");
            batch[4].Id.Should().Be("cmd5");

            // Verify GetKeyForItem
            batch["cmd1"].Raw.Should().Be("raw_command1");
            batch["cmd2"].Raw.Should().Be("raw_command2");
            batch["cmd3"].Raw.Should().Be("raw_command3");
            batch["cmd4"].Raw.Should().Be("raw_command4");
            batch["cmd5"].Raw.Should().Be("raw_command5");
        }
    }
}
