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
        public async Task AddAsync_Batch_Commands_HandlesConcurrentCalls()
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = true;

            // Mock the setup for the command router
            List<string> routedCommands = [];
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(c => routedCommands.Add(c.Request.Raw));

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object);
            int idx = 0;
            var tasks = Enumerable.Range(0, 500).Select<int, Task>(e =>
            {
                ++idx;
                string batch = TerminalServices.CreateBatch(_mockOptions.Object.Value, [$"command_{idx}_0", $"command_{idx}_1", $"command_{idx}_2"]);
                return _terminalProcessor.AddRequestAsync(batch, "sender", "endpoint");
            });
            await Task.WhenAll(tasks);

            await Task.Delay(500);

            // Assert all were processed without error
            routedCommands.Distinct().Should().HaveCount(1500);
        }

        [Fact]
        public async Task AddAsync_CallsExceptionHandler_WhenRouterFails()
        {
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>())).ThrowsAsync(new Exception("Router error"));

            // Act
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object);
            await _terminalProcessor.AddRequestAsync("command1|", "sender", "endpoint");
            await Task.Delay(500);

            // Assert exception handler was called
            _mockExceptionHandler.Verify(e => e.HandleExceptionAsync(It.IsAny<TerminalExceptionHandlerContext>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_Does_Add_When_BatchDelimiter_Missing_In_Non_BatchMode()
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = false;

            // Act
            await _terminalProcessor.AddRequestAsync("command1", "sender", "endpoint");

            // Assert only a single command was processed
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(1);
            _terminalProcessor.UnprocessedRequests.Any(r => r.Raw == "command1").Should().BeTrue();
        }

        [Fact]
        public async Task AddAsync_Processes_BatchCommand_In_Order_WhenBatchModeEnabled()
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = true;
            _mockOptions.Object.Value.Router.RemoteBatchMaxLength = 1500000;

            // Create sets of commands to simulate batch processing
            List<string> commands1 = new(Enumerable.Range(0, 1000).Select(i => $"command_1_{i}"));
            List<string> commands2 = new(Enumerable.Range(0, 1000).Select(i => $"command_2_{i}"));
            List<string> commands3 = new(Enumerable.Range(0, 1000).Select(i => $"command_3_{i}"));
            List<string> commands4 = new(Enumerable.Range(0, 1000).Select(i => $"command_4_{i}"));
            List<string> commands5 = new(Enumerable.Range(0, 1000).Select(i => $"command_5_{i}"));
            List<string> commands6 = new(Enumerable.Range(0, 1000).Select(i => $"command_6_{i}"));
            List<string> commands7 = new(Enumerable.Range(0, 1000).Select(i => $"command_7_{i}"));
            List<string> commands8 = new(Enumerable.Range(0, 1000).Select(i => $"command_8_{i}"));
            List<string> commands9 = new(Enumerable.Range(0, 1000).Select(i => $"command_9_{i}"));
            List<string> commands10 = new(Enumerable.Range(0, 1000).Select(i => $"command_10_{i}"));

            // Create batches for each command collection
            var batch1 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands1.ToArray());
            var batch2 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands2.ToArray());
            var batch3 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands3.ToArray());
            var batch4 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands4.ToArray());
            var batch5 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands5.ToArray());
            var batch6 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands6.ToArray());
            var batch7 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands7.ToArray());
            var batch8 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands8.ToArray());
            var batch9 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands9.ToArray());
            var batch10 = TerminalServices.CreateBatch(_mockOptions.Object.Value, commands10.ToArray());

            // Add all batches asynchronously
            Task addBatch1 = _terminalProcessor.AddRequestAsync(batch1, "sender1", "endpoint1");
            Task addBatch2 = _terminalProcessor.AddRequestAsync(batch2, "sender2", "endpoint2");
            Task addBatch3 = _terminalProcessor.AddRequestAsync(batch3, "sender3", "endpoint3");
            Task addBatch4 = _terminalProcessor.AddRequestAsync(batch4, "sender4", "endpoint4");
            Task addBatch5 = _terminalProcessor.AddRequestAsync(batch5, "sender5", "endpoint5");
            Task addBatch6 = _terminalProcessor.AddRequestAsync(batch6, "sender6", "endpoint6");
            Task addBatch7 = _terminalProcessor.AddRequestAsync(batch7, "sender7", "endpoint7");
            Task addBatch8 = _terminalProcessor.AddRequestAsync(batch8, "sender8", "endpoint8");
            Task addBatch9 = _terminalProcessor.AddRequestAsync(batch9, "sender9", "endpoint9");
            Task addBatch10 = _terminalProcessor.AddRequestAsync(batch10, "sender10", "endpoint10");

            // Wait for all batches to be processed
            await Task.WhenAll(addBatch1, addBatch2, addBatch3, addBatch4, addBatch5, addBatch6, addBatch7, addBatch8, addBatch9, addBatch10);

            // Verify all commands are processed
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(10000);

            // Collect the processed commands into groups by their prefixes (e.g., "command_1_", "command_2_", etc.)
            var groupedCommands = _terminalProcessor.UnprocessedRequests
                .GroupBy(r => r.Raw.Split('_')[1])
                .ToDictionary(g => g.Key, g => g.Select(r => r.Raw).ToList());

            // Verify that each group of commands is in the expected order
            groupedCommands["1"].Should().BeEquivalentTo(commands1, options => options.WithStrictOrdering());
            groupedCommands["2"].Should().BeEquivalentTo(commands2, options => options.WithStrictOrdering());
            groupedCommands["3"].Should().BeEquivalentTo(commands3, options => options.WithStrictOrdering());
            groupedCommands["4"].Should().BeEquivalentTo(commands4, options => options.WithStrictOrdering());
            groupedCommands["5"].Should().BeEquivalentTo(commands5, options => options.WithStrictOrdering());
            groupedCommands["6"].Should().BeEquivalentTo(commands6, options => options.WithStrictOrdering());
            groupedCommands["7"].Should().BeEquivalentTo(commands7, options => options.WithStrictOrdering());
            groupedCommands["8"].Should().BeEquivalentTo(commands8, options => options.WithStrictOrdering());
            groupedCommands["9"].Should().BeEquivalentTo(commands9, options => options.WithStrictOrdering());
            groupedCommands["10"].Should().BeEquivalentTo(commands10, options => options.WithStrictOrdering());
        }

        [Theory]
        [InlineData("command1,command2,command3,command4,command5,command6|")]
        [InlineData("command1,command2,command3,command4,command5,command6,|")]
        [InlineData("command1,,,command2,command3,command4,command5,command6|")]
        public async Task AddAsync_Processes_BatchCommands_By_Ignoring_Empty_Commands(string message)
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = true;

            // Act
            await _terminalProcessor.AddRequestAsync(message, "sender", "endpoint");
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(6);
            _terminalProcessor.UnprocessedRequests.Any(r => r.Raw == "command1").Should().BeTrue();
            _terminalProcessor.UnprocessedRequests.Any(r => r.Raw == "command2").Should().BeTrue();
            _terminalProcessor.UnprocessedRequests.Any(r => r.Raw == "command3").Should().BeTrue();
            _terminalProcessor.UnprocessedRequests.Any(r => r.Raw == "command4").Should().BeTrue();
            _terminalProcessor.UnprocessedRequests.Any(r => r.Raw == "command5").Should().BeTrue();
            _terminalProcessor.UnprocessedRequests.Any(r => r.Raw == "command6").Should().BeTrue();
        }

        [Fact]
        public async Task AddAsync_Processes_VeryLarge_BatchCommand_WhenBatchModeEnabled()
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = true;
            _mockOptions.Object.Value.Router.RemoteBatchMaxLength = 1500000;

            // Send batch of 100000 commands by using TerminalServices
            HashSet<string> allCommands = new(Enumerable.Range(0, 100000).Select(i => $"command{i}"));
            var longBatch = TerminalServices.CreateBatch(_mockOptions.Object.Value, allCommands.ToArray());
            await _terminalProcessor.AddRequestAsync(longBatch, "sender", "endpoint");

            // We are iterating over a large number of unprocessed requests, so we need to ensure that the validation
            // code is not too slow. We are also checking that all commands are present in the batch at the same time
            // reducing the batch size so that the test does not take too long to run.
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(100000);
            foreach (var request in _terminalProcessor.UnprocessedRequests)
            {
                if (allCommands.Contains(request.Raw))
                {
                    allCommands.Remove(request.Raw);
                }
                else
                {
                    throw new InvalidOperationException($"An unexpected command was added to the unprocessed requests. command={request}");
                }
            }
            allCommands.Should().BeEmpty("All commands are accounted for.");
        }

        [Fact]
        public async Task AddAsync_Single_Commands_HandlesConcurrentCalls()
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = false;

            // Mock the setup for the command router
            List<string> routedCommands = [];
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>()))
                .Callback<CommandRouterContext>(c => routedCommands.Add(c.Request.Raw));

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object);
            int idx = 1;
            var tasks = Enumerable.Range(0, 500).Select<int, Task>(e =>
            {
                return _terminalProcessor.AddRequestAsync($"command{idx++}", "sender", "endpoint");
            });
            await Task.WhenAll(tasks);

            await Task.Delay(500);

            // Assert all were processed without error
            routedCommands.Distinct().Should().HaveCount(500);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task AddAsync_Throws_On_EmptyBatch(string? batch)
        {
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = false;
            Func<Task> act = () => _terminalProcessor.AddRequestAsync(batch!, "sender", "endpoint");
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The command or batch cannot be empty.");

            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = true;
            act = () => _terminalProcessor.AddRequestAsync(batch!, "sender", "endpoint");
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The command or batch cannot be empty.");
        }

        [Fact]
        public async Task AddAsync_Throws_When_BatchDelimiter_Is_Larger_Than_Batch_In_BatchMode()
        {
            // Make sure delimter is enabled and greater than 1 character
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = true;
            _mockOptions.Object.Value.Router.RemoteBatchDelimiter = "$m$";

            // Act
            Func<Task> act = () => _terminalProcessor.AddRequestAsync("c", "sender", "endpoint");
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The raw batch must end with the batch delimiter.");

            // Assert only a single command was processed
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(0);
        }

        [Theory]
        [InlineData("command1")]
        [InlineData("|command1")]
        [InlineData("command1||")]
        [InlineData("com|mand1")]
        [InlineData("|com|mand1")]
        [InlineData("|command1|")]
        [InlineData("command1|command2|")]
        [InlineData("|command1|command2|")]
        [InlineData("|command1|command2")]
        public async Task AddAsync_Throws_When_BatchDelimiter_Is_Misplaced_In_BatchMode(string batch)
        {
            // Make sure delimter is enabled and greater than 1 character
            _mockOptions.Object.Value.Router.EnableRemoteDelimiters = true;

            // Act
            Func<Task> act = () => _terminalProcessor.AddRequestAsync(batch, "sender", "endpoint");
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The raw batch must have a single delimiter at the end, not missing or placed elsewhere.");

            // Assert only a single command was processed
            _terminalProcessor.UnprocessedRequests.Should().HaveCount(0);
        }

        [Fact]
        public async Task AddAsync_ThrowsException_WhenBatchTooLong()
        {
            _mockOptions.Object.Value.Router.RemoteBatchMaxLength = 1000;

            var longBatch = new string('A', 1001);
            Func<Task> act = () => _terminalProcessor.AddRequestAsync(longBatch, "sender", "endpoint");
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_configuration")
                .WithErrorDescription("The batch length exceeds configured maximum. max_length=1000");
        }

        [Fact]
        public void ByDefault_Processor_Is_NotProcessing()
        {
            _terminalProcessor.IsProcessing.Should().BeFalse();
        }

        [Fact]
        public void NewUniqueId_Generates_Unique_Id()
        {
            string testId = _terminalProcessor.NewUniqueId();
            Guid.TryParse(testId, out _).Should().BeTrue();

            testId = _terminalProcessor.NewUniqueId(null);
            Guid.TryParse(testId, out _).Should().BeTrue();

            testId = _terminalProcessor.NewUniqueId("   ");
            Guid.TryParse(testId, out _).Should().BeTrue();

            testId = _terminalProcessor.NewUniqueId("blah");
            Guid.TryParse(testId, out _).Should().BeTrue();

            for (int idx = 0; idx < 100; ++idx)
            {
                testId = _terminalProcessor.NewUniqueId("batch");
                testId.Should().Be($"b{idx}");
            }

            for (int idx = 0; idx < 1000; ++idx)
            {
                testId = _terminalProcessor.NewUniqueId("client");
                testId.Should().Be($"c{idx}");
            }

            for (int idx = 0; idx < 10000; ++idx)
            {
                testId = _terminalProcessor.NewUniqueId("route");
                testId.Should().Be($"r{idx}");
            }
        }

        [Fact]
        public async Task StartProcessing_HandlesRouterException()
        {
            Exception? handeledException = null;
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>())).ThrowsAsync(new Exception("Router error"));
            _mockExceptionHandler.Setup(e => e.HandleExceptionAsync(It.IsAny<TerminalExceptionHandlerContext>())).Callback<TerminalExceptionHandlerContext>(c => handeledException = c.Exception);

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object);
            await _terminalProcessor.AddRequestAsync("command1|", "sender", "endpoint");
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
            await _terminalProcessor.AddRequestAsync("command1|", "sender_1", "sender_endpoint_1");
            await Task.Delay(500);

            routeContext.Should().NotBeNull();
            routeContext!.Properties.Should().HaveCount(2);
            routeContext.Properties!["sender_endpoint"].Should().Be("sender_endpoint_1");
            routeContext.Properties!["sender_id"].Should().Be("sender_1");

            routeContext.Request.Id.Should().NotBeNullOrWhiteSpace();
            routeContext.Request.Raw.Should().Be("command1");

            routeContext.TerminalContext.Should().BeSameAs(_mockTerminalRouterContext.Object);

            // Add without sender endpoint and sender id
            routeContext = null;
            await _terminalProcessor.AddRequestAsync("command2|", null, null);
            await Task.Delay(500);

            routeContext.Should().NotBeNull();
            routeContext!.Properties.Should().HaveCount(1);
            routeContext.Properties!["sender_endpoint"].Should().Be("$unknown$");

            routeContext.Request.Id.Should().NotBeNullOrWhiteSpace();
            routeContext.Request.Raw.Should().Be("command2");

            routeContext.TerminalContext.Should().BeSameAs(_mockTerminalRouterContext.Object);
        }

        [Fact]
        public async Task StopProcessing_With_Add_Does_Not_Routes_Command_To_Router()
        {
            CommandRouterContext? routeContext = null;
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandRouterContext>())).Callback<CommandRouterContext>(c => routeContext = c);

            // Add with sender endpoint and sender id
            await _terminalProcessor.AddRequestAsync("command1|", "sender_1", "sender_endpoint_1");
            await Task.Delay(500);
            routeContext.Should().BeNull();

            // Add without sender endpoint and sender id
            await _terminalProcessor.AddRequestAsync("command2|", null, null);
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
