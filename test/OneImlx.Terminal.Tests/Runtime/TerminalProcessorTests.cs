/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Moq;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Runtime
{
    public class TerminalProcessorTests
    {
        public TerminalProcessorTests()
        {
            _terminalTokenSource = new CancellationTokenSource();
            terminalStartContext = new TerminalStartContext(TerminalStartMode.Console, _terminalTokenSource.Token, CancellationToken.None, null, null);
            _mockCommandRouter = new Mock<ICommandRouter>();
            _mockExceptionHandler = new Mock<ITerminalExceptionHandler>();
            _mockLogger = new Mock<ILogger<TerminalProcessor>>();
            _mockOptions = new Mock<IOptions<TerminalOptions>>();
            _mockTerminalRouterContext = new Mock<TerminalRouterContext>(terminalStartContext);
            _mockTextHandler = new Mock<ITerminalTextHandler>();

            _mockOptions.Setup(o => o.Value).Returns(new TerminalOptions
            {
                Router = new RouterOptions
                {
                    RemoteBatchMaxLength = 1000,
                    EnableRemoteDelimiters = true,
                    RemoteBatchDelimiter = "|",
                    RemoteCommandDelimiter = ",",
                    Timeout = 1000
                }
            });

            _terminalProcessor = new TerminalProcessor(
                _mockCommandRouter.Object,
                _mockExceptionHandler.Object,
                _mockOptions.Object,
                _mockTextHandler.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task AddAsync_CallsExceptionHandler_WhenRouterFails()
        {
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>())).ThrowsAsync(new Exception("Router error"));

            // Act
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object);
            await _terminalProcessor.AddAsync("command1|", "endpoint", "sender");
            await Task.Delay(500);

            // Assert exception handler was called
            _mockExceptionHandler.Verify(e => e.HandleExceptionAsync(It.IsAny<TerminalExceptionHandlerContext>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_Does_Add_When_BatchDelimiter_Missing_In_Non_BatchMode()
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = false;

            // Act
            await _terminalProcessor.AddAsync("command1", "endpoint", "sender");

            // Assert only a single command was processed
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(1);
            _terminalProcessor.UnprocessedRequests.Any(r => r.CommandString == "command1").Should().BeTrue();
        }

        [Fact]
        public async Task AddAsync_DoesNot_Add_When_BatchDelimiter_Missing_In_BatchMode()
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = true;

            // Act
            await _terminalProcessor.AddAsync("command1", "endpoint", "sender");

            // Assert only a single command was processed
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(0);
        }

        [Fact]
        public async Task AddAsync_HandlesConcurrentCalls()
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = false;

            // Mock the setup for the command router
            List<string> routedCommands = [];
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(c => routedCommands.Add(c.Route.Raw));

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object);
            int idx = 1;
            var tasks = Enumerable.Range(0, 500).Select<int, Task>(e =>
            {
                return _terminalProcessor.AddAsync($"command{idx++}", "endpoint", "sender");
            });
            await Task.WhenAll(tasks);

            await Task.Delay(500);

            // Assert all were processed without error
            routedCommands.Distinct().Should().HaveCount(500);
        }

        [Theory]
        [InlineData("command1,command2,command3,command4,command5,command6|")]
        [InlineData("command1,command2,command3,command4,command5,command6,|")]
        [InlineData("command1,|command2,command3,|command4,command5,command6|")]
        [InlineData("command1,,,command2,command3,|command4,command5,command6|")]
        [InlineData("command1,command2,command3,|||||command4,command5,command6|")]
        [InlineData("command1|command2|command3|command4|command5|command6|")]
        public async Task AddAsync_Processes_BatchCommands_WhenBatchModeEnabled(string message)
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = true;

            // Act
            await _terminalProcessor.AddAsync(message, "endpoint", "sender");
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(6);
            _terminalProcessor.UnprocessedRequests.Any(r => r.CommandString == "command1").Should().BeTrue();
            _terminalProcessor.UnprocessedRequests.Any(r => r.CommandString == "command2").Should().BeTrue();
            _terminalProcessor.UnprocessedRequests.Any(r => r.CommandString == "command3").Should().BeTrue();
            _terminalProcessor.UnprocessedRequests.Any(r => r.CommandString == "command4").Should().BeTrue();
            _terminalProcessor.UnprocessedRequests.Any(r => r.CommandString == "command5").Should().BeTrue();
            _terminalProcessor.UnprocessedRequests.Any(r => r.CommandString == "command6").Should().BeTrue();
        }

        [Fact]
        public async Task AddAsync_Processes_MultipleBatch_SplitInto_Chunks_Independently()
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = true;
            _mockOptions.Object.Value.Router.RemoteBatchMaxLength = 4780000;

            // Create sets of commands
            HashSet<string> commands1 = new(Enumerable.Range(0, 100000).Select(i => $"command_1.{i}"));
            HashSet<string> commands2 = new(Enumerable.Range(0, 100000).Select(i => $"command_2.{i}"));
            HashSet<string> commands3 = new(Enumerable.Range(0, 100000).Select(i => $"command_3.{i}"));
            HashSet<string> allCommands = new(commands1.Concat(commands2).Concat(commands3));

            // Create 3 batches
            var longBatch1 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands1.ToArray());
            var longBatch2 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands2.ToArray());
            var longBatch3 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands3.ToArray());

            // Create a single combined batch from all commands
            var combinedBatch = longBatch1 + longBatch2 + longBatch3;

            // Split the combined batch string into 20 equal parts
            int partSize = combinedBatch.Length / 20;
            var batchChunks = new string[20];
            for (int i = 0; i < 20; i++)
            {
                int startIndex = i * partSize;
                int length = (i == 19) ? combinedBatch.Length - startIndex : partSize;
                batchChunks[i] = combinedBatch.Substring(startIndex, length);
            }

            // Send each chunk independently via AddAsync
            foreach (var chunk in batchChunks)
            {
                await _terminalProcessor.AddAsync(chunk, "endpoint", "sender");
            }

            // Verify all commands are in the unprocessed requests
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(300000);
            foreach (var request in _terminalProcessor.UnprocessedRequests)
            {
                if (allCommands.Contains(request.CommandString))
                {
                    allCommands.Remove(request.CommandString);
                }
                else
                {
                    throw new InvalidOperationException($"An unexpected command was added to the unprocessed requests. command={request}");
                }
            }
            allCommands.Should().BeEmpty("All commands are accounted for.");
        }

        [Fact]
        public async Task AddAsync_Processes_SingleBatch_SplitInto_Chunks_Independently()
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = true;
            _mockOptions.Object.Value.Router.RemoteBatchMaxLength = 4780000;

            // Create sets of commands
            HashSet<string> commands1 = new(Enumerable.Range(0, 100000).Select(i => $"command_1.{i}"));
            HashSet<string> commands2 = new(Enumerable.Range(0, 100000).Select(i => $"command_2.{i}"));
            HashSet<string> commands3 = new(Enumerable.Range(0, 100000).Select(i => $"command_3.{i}"));
            HashSet<string> allCommands = new(commands1.Concat(commands2).Concat(commands3));

            // Create a single combined batch from all commands
            var combinedBatch = TerminalServices.CreateBatch(_mockOptions.Object.Value, allCommands.ToArray());

            // Split the combined batch string into 20 equal parts
            int partSize = combinedBatch.Length / 20;
            var batchChunks = new string[20];
            for (int i = 0; i < 20; i++)
            {
                int startIndex = i * partSize;
                int length = (i == 19) ? combinedBatch.Length - startIndex : partSize;
                batchChunks[i] = combinedBatch.Substring(startIndex, length);
            }

            // Send each chunk independently via AddAsync
            foreach (var chunk in batchChunks)
            {
                await _terminalProcessor.AddAsync(chunk, "endpoint", "sender");
            }

            // Verify all commands are in the unprocessed requests
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(300000);
            foreach (var request in _terminalProcessor.UnprocessedRequests)
            {
                if (allCommands.Contains(request.CommandString))
                {
                    allCommands.Remove(request.CommandString);
                }
                else
                {
                    throw new InvalidOperationException($"An unexpected command was added to the unprocessed requests. command={request}");
                }
            }
            allCommands.Should().BeEmpty("All commands are accounted for.");
        }

        [Fact]
        public async Task AddAsync_Processes_VeryLarge_BatchCommand_WhenBatchModeEnabled()
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = true;
            _mockOptions.Object.Value.Router.RemoteBatchMaxLength = 1500000;

            // Send batch of 100000 commands by using TerminalServices
            HashSet<string> allCommands = new(Enumerable.Range(0, 100000).Select(i => $"command{i}"));
            var longBatch = TerminalServices.CreateBatch(_mockOptions.Object.Value, allCommands.ToArray());
            await _terminalProcessor.AddAsync(longBatch, "endpoint", "sender");

            // We are iterating over a large number of unprocessed requests, so we need to ensure that the validation
            // code is not too slow. We are also checking that all commands are present in the batch at the same time
            // reducing the batch size so that the test does not take too long to run.
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(100000);
            foreach (var request in _terminalProcessor.UnprocessedRequests)
            {
                if (allCommands.Contains(request.CommandString))
                {
                    allCommands.Remove(request.CommandString);
                }
                else
                {
                    throw new InvalidOperationException($"An unexpected command was added to the unprocessed requests. command={request}");
                }
            }
            allCommands.Should().BeEmpty("All commands are accounted for.");
        }

        [Fact]
        public async Task AddAsync_Processes_VeryLarge_MultipleBatches_WhenBatchModeEnabled()
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = true;
            _mockOptions.Object.Value.Router.RemoteBatchMaxLength = 4780000;

            // Send batch of 100000 commands by using TerminalServices
            HashSet<string> commands1 = new(Enumerable.Range(0, 100000).Select(i => $"command_1.{i}"));
            HashSet<string> commands2 = new(Enumerable.Range(0, 100000).Select(i => $"command_2.{i}"));
            HashSet<string> commands3 = new(Enumerable.Range(0, 100000).Select(i => $"command_3.{i}"));
            HashSet<string> allCommands = new(commands1.Concat(commands2).Concat(commands3));

            // Create 3 batches
            var longBatch1 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands1.ToArray());
            var longBatch2 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands2.ToArray());
            var longBatch3 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands3.ToArray());

            // Now concatenate the 3 batches
            var longBatch = longBatch1 + longBatch2 + longBatch3;
            await _terminalProcessor.AddAsync(longBatch, "endpoint", "sender");

            // We are iterating over a large number of unprocessed requests, so we need to ensure that the validation
            // code is not too slow. We are also checking that all commands are present in the batch at the same time
            // reducing the batch size so that the test does not take too long to run.
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(300000);
            foreach (var request in _terminalProcessor.UnprocessedRequests)
            {
                if (allCommands.Contains(request.CommandString))
                {
                    allCommands.Remove(request.CommandString);
                }
                else
                {
                    throw new InvalidOperationException($"An unexpected command was added to the unprocessed requests. command={request}");
                }
            }
            allCommands.Should().BeEmpty("All commands are accounted for.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task AddAsync_Throws_On_EmptyBatch(string? batch)
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = false;
            Func<Task> act = () => _terminalProcessor.AddAsync(batch!, "endpoint", "sender");
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The batch cannot be empty.");

            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = true;
            act = () => _terminalProcessor.AddAsync(batch!, "endpoint", "sender");
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The batch cannot be empty.");
        }

        [Fact]
        public async Task AddAsync_ThrowsException_WhenBatchTooLong()
        {
            _mockOptions.Object.Value.Router.RemoteBatchMaxLength = 1000;

            var longBatch = new string('A', 1001);
            Func<Task> act = () => _terminalProcessor.AddAsync(longBatch, "endpoint", "sender");
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_configuration")
                .WithErrorDescription("Batch length exceeds configured maximum. max_length=1000");
        }

        [Fact]
        public void ByDefault_Processor_Is_NotProcessing()
        {
            _terminalProcessor.IsProcessing.Should().BeFalse();
        }

        [Fact]
        public async Task StartProcessing_HandlesRouterException()
        {
            Exception? handeledException = null;
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>())).ThrowsAsync(new Exception("Router error"));
            _mockExceptionHandler.Setup(e => e.HandleExceptionAsync(It.IsAny<TerminalExceptionHandlerContext>())).Callback<TerminalExceptionHandlerContext>(c => handeledException = c.Exception);

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object);
            await _terminalProcessor.AddAsync("command1|", "endpoint", "sender");
            await Task.Delay(500);

            handeledException.Should().NotBeNull();
            handeledException!.Message.Should().Be("Router error");
        }

        [Fact]
        public void StartProcessing_Sets_IsProcessing_ToTrue()
        {
            _terminalProcessor.IsProcessing.Should().BeFalse();
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object);
            _terminalProcessor.IsProcessing.Should().BeTrue();
            _terminalTokenSource.Cancel();
        }

        [Fact]
        public async Task StartProcessing_With_Add_Routes_Command_To_Router()
        {
            CommandRouterContext? routeContext = null;
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>())).Callback<CommandRouterContext>(c => routeContext = c);

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object);

            // Add with sender endpoint and sender id
            await _terminalProcessor.AddAsync("command1|", "sender_endpoint_1", "sender_1");
            await Task.Delay(500);

            routeContext.Should().NotBeNull();
            routeContext!.Properties.Should().HaveCount(2);
            routeContext.Properties!["sender_endpoint"].Should().Be("sender_endpoint_1");
            routeContext.Properties!["sender_id"].Should().Be("sender_1");

            routeContext.Route.Id.Should().NotBeNullOrWhiteSpace();
            routeContext.Route.Raw.Should().Be("command1");

            routeContext.TerminalContext.Should().BeSameAs(_mockTerminalRouterContext.Object);

            // Add without sender endpoint and sender id
            routeContext = null;
            await _terminalProcessor.AddAsync("command2|", null, null);
            await Task.Delay(500);

            routeContext.Should().NotBeNull();
            routeContext!.Properties.Should().HaveCount(1);
            routeContext.Properties!["sender_endpoint"].Should().Be("$unknown$");

            routeContext.Route.Id.Should().NotBeNullOrWhiteSpace();
            routeContext.Route.Raw.Should().Be("command2");

            routeContext.TerminalContext.Should().BeSameAs(_mockTerminalRouterContext.Object);
        }

        [Fact]
        public async Task StopProcessing_With_Add_Does_Not_Routes_Command_To_Router()
        {
            CommandRouterContext? routeContext = null;
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>())).Callback<CommandRouterContext>(c => routeContext = c);

            // Add with sender endpoint and sender id
            await _terminalProcessor.AddAsync("command1|", "sender_endpoint_1", "sender_1");
            await Task.Delay(500);
            routeContext.Should().BeNull();

            // Add without sender endpoint and sender id
            await _terminalProcessor.AddAsync("command2|", null, null);
            await Task.Delay(500);
            routeContext.Should().BeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1000)]
        [InlineData(5000)]
        [InlineData(Timeout.Infinite)]
        public async Task StopProcessingAsync_Any_Timeout_And_Completed_Processing_Sets_IsProcessing_ToFalse(int timeout)
        {
            var context = new Mock<TerminalRouterContext>(terminalStartContext).Object;

            _terminalProcessor.StartProcessing(context);
            _terminalProcessor.IsProcessing.Should().BeTrue();
            _terminalTokenSource.Cancel();
            await Task.Delay(500);
            var timedOut = await _terminalProcessor.StopProcessingAsync(timeout);
            timedOut.Should().BeFalse();
            _terminalProcessor.IsProcessing.Should().BeFalse();
        }

        [Fact]
        public async Task StopProcessingAsync_Does_Not_Stop_If_Not_Processing()
        {
            _terminalProcessor.IsProcessing.Should().BeFalse();
            bool timeout = await _terminalProcessor.StopProcessingAsync(100);
            _terminalProcessor.IsProcessing.Should().BeFalse();
            timeout.Should().BeFalse();
        }

        [Fact]
        public async Task StopProcessingAsync_Sets_IsProcessing_ToFalse()
        {
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object);
            _terminalTokenSource.CancelAfter(300);

            _terminalProcessor.IsProcessing.Should().BeTrue();
            bool timeout = await _terminalProcessor.StopProcessingAsync(Timeout.Infinite);
            _terminalProcessor.IsProcessing.Should().BeFalse();
            timeout.Should().BeFalse();
        }

        [Fact]
        public async Task StopProcessingAsync_TimesOut_Return_True_Sets_IsProcessing_ToTrue()
        {
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object);
            _terminalTokenSource.CancelAfter(400);
            _terminalProcessor.IsProcessing.Should().BeTrue();
            bool timeout = await _terminalProcessor.StopProcessingAsync(100);
            _terminalProcessor.IsProcessing.Should().BeTrue();
            timeout.Should().BeTrue();

            // give time for the processor to stop
            await Task.Delay(500);
        }

        [Fact]
        public async Task WaitAsync_CancelsIndefiniteProcessing()
        {
            // Start the WaitAsync task
            Task waiting = _terminalProcessor.WaitAsync(_mockTerminalRouterContext.Object);

            // Check periodically to ensure the task is not yet completed
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(100);
                waiting.Status.Should().NotBe(TaskStatus.RanToCompletion);
            }

            // Trigger cancellation and verify the task completes
            _terminalTokenSource.Cancel();
            await Task.Delay(200);
            waiting.Status.Should().Be(TaskStatus.RanToCompletion);
        }

        private readonly Mock<ICommandRouter> _mockCommandRouter;
        private readonly Mock<ITerminalExceptionHandler> _mockExceptionHandler;
        private readonly Mock<ILogger<TerminalProcessor>> _mockLogger;
        private readonly Mock<IOptions<TerminalOptions>> _mockOptions;
        private readonly Mock<TerminalRouterContext> _mockTerminalRouterContext;
        private readonly Mock<ITerminalTextHandler> _mockTextHandler;
        private readonly TerminalProcessor _terminalProcessor;
        private readonly CancellationTokenSource _terminalTokenSource;
        private readonly TerminalStartContext terminalStartContext;
    }
}
