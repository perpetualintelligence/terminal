/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OneImlx.Shared.Infrastructure;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Test.FluentAssertions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Runtime
{
    public class TerminalProcessorTests
    {
        public TerminalProcessorTests()
        {
            _terminalTokenSource = new CancellationTokenSource();
            _mockCommandRouter = new Mock<ICommandRouter>();
            _mockExceptionHandler = new Mock<ITerminalExceptionHandler>();
            _mockLogger = new Mock<ILogger<TerminalProcessor>>();
            _mockOptions = new Mock<IOptions<TerminalOptions>>();
            _mockTerminalRouterContext = new Mock<TerminalRouterContext>(TerminalStartMode.Console, _terminalTokenSource.Token, CancellationToken.None, null!, null!);
            _textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII);

            _mockOptions.Setup(static o => o.Value).Returns(new TerminalOptions
            {
                Router = new RouterOptions
                {
                    MaxLength = 1000,
                    Timeout = 1000
                }
            });

            _terminalProcessor = new TerminalProcessor(
                _mockCommandRouter.Object,
                _mockExceptionHandler.Object,
                _mockOptions.Object,
                _textHandler,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Add_Without_Processing_And_Background_Throws()
        {
            // Add with sender endpoint and sender id
            Func<Task> act = async () => await _terminalProcessor.AddAsync(TerminalInputOutput.Single("id1", "command1", "sender_1", "sender_endpoint_1"));
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The terminal processor is not running.");

            // Start but without background
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: false);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The terminal processor is not running a background queue.");
        }

        [Fact]
        public async Task AddAsync_Batch_Commands_HandlesConcurrentCalls()
        {
            // Mock the setup for the command router
            List<string> routedCommands = [];
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandContext>()))
                .Callback<CommandContext>(c => routedCommands.Add(c.Request.Raw))
                .ReturnsAsync(new CommandResult());

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);
            int idx = 0;
            var tasks = Enumerable.Range(0, 500).Select(e =>
            {
                ++idx;
                string[] cmdIds = [$"id_{idx}_0", $"id_{idx}_1", $"id_{idx}_2"];
                string[] cmds = [$"command_{idx}_0", $"command_{idx}_1", $"command_{idx}_2"];
                TerminalInputOutput batch = TerminalInputOutput.Batch("batch1", cmdIds, cmds, "sender", "endpoint");
                return _terminalProcessor.AddAsync(batch);
            });
            await Task.WhenAll(tasks);

            await Task.Delay(500);

            // Assert all were processed without error
            routedCommands.Distinct().Should().HaveCount(1500);
        }

        [Fact]
        public async Task AddAsync_CallsExceptionHandler_WhenRouterFails()
        {
            _mockCommandRouter.Setup(static r => r.RouteCommandAsync(It.IsAny<CommandContext>())).ThrowsAsync(new Exception("Router error"));

            // Act
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);
            await _terminalProcessor.AddAsync(TerminalInputOutput.Single("id1", "command1", "sender", "endpoint"));
            await Task.Delay(500);

            // Assert exception handler was called
            _mockExceptionHandler.Verify(static e => e.HandleExceptionAsync(It.IsAny<TerminalExceptionHandlerContext>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_Does_Add_When_BatchDelimiter_Missing_In_Non_BatchMode()
        {
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Setup that the mock command router was invoked
            List<string> routedCommands = [];
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandContext>()))
                .Callback<CommandContext>(c => routedCommands.Add(c.Request.Raw));

            // Act
            await _terminalProcessor.AddAsync(TerminalInputOutput.Single("id1", "command1", "sender", "endpoint"));
            await Task.Delay(500);

            // Assert only a single command was processed
            routedCommands.Should().HaveCount(1);
            routedCommands.Should().Contain("command1");
        }

        [Fact]
        public async Task AddAsync_Handles_ConcurrentCalls()
        {
            // Mock the setup for the command router
            List<string> routedCommands = [];
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandContext>()))
                .Callback<CommandContext>(c => routedCommands.Add(c.Request.Raw));

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);
            int idx = 1;
            var tasks = Enumerable.Range(0, 500).Select(e =>
            {
                return _terminalProcessor.AddAsync(TerminalInputOutput.Single($"id{idx++}", $"command{idx++}", "sender", "endpoint"));
            });
            await Task.WhenAll(tasks);

            await Task.Delay(500);

            // Assert all were processed without error
            routedCommands.Distinct().Should().HaveCount(500);
        }

        [Fact]
        public async Task AddAsync_Processes_BatchCommand_In_Order()
        {
            _mockOptions.Object.Value.Router.MaxLength = 1500000;
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Setup that the mock command router was invoked
            List<string> routedCommands = [];
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandContext>()))
                .Callback<CommandContext>(c => routedCommands.Add(c.Request.Raw))
                .ReturnsAsync(new CommandResult());

            OrderedDictionary commands1 = [];
            for (int i = 0; i < 1000; i++)
            {
                commands1.Add($"id_1_{i}", $"command_1_{i}");
            }

            OrderedDictionary commands2 = [];
            for (int i = 0; i < 1000; i++)
            {
                commands2.Add($"id_2_{i}", $"command_2_{i}");
            }

            OrderedDictionary commands3 = [];
            for (int i = 0; i < 1000; i++)
            {
                commands3.Add($"id_3_{i}", $"command_3_{i}");
            }

            OrderedDictionary commands4 = [];
            for (int i = 0; i < 1000; i++)
            {
                commands4.Add($"id_4_{i}", $"command_4_{i}");
            }

            OrderedDictionary commands5 = [];
            for (int i = 0; i < 1000; i++)
            {
                commands5.Add($"id_5_{i}", $"command_5_{i}");
            }

            OrderedDictionary commands6 = [];
            for (int i = 0; i < 1000; i++)
            {
                commands6.Add($"id_6_{i}", $"command_6_{i}");
            }

            OrderedDictionary commands7 = [];
            for (int i = 0; i < 1000; i++)
            {
                commands7.Add($"id_7_{i}", $"command_7_{i}");
            }

            OrderedDictionary commands8 = [];
            for (int i = 0; i < 1000; i++)
            {
                commands8.Add($"id_8_{i}", $"command_8_{i}");
            }

            OrderedDictionary commands9 = [];
            for (int i = 0; i < 1000; i++)
            {
                commands9.Add($"id_9_{i}", $"command_9_{i}");
            }

            OrderedDictionary commands10 = [];
            for (int i = 0; i < 1000; i++)
            {
                commands10.Add($"id_10_{i}", $"command_10_{i}");
            }

            // Create batches for each command collection
            var batch1 = TerminalInputOutput.Batch("batch1", commands1.Keys.Cast<string>().ToArray(), commands1.Values.Cast<string>().ToArray(), "sender1", "endpoint1");
            var batch2 = TerminalInputOutput.Batch("batch2", commands2.Keys.Cast<string>().ToArray(), commands2.Values.Cast<string>().ToArray(), "sender2", "endpoint2");
            var batch3 = TerminalInputOutput.Batch("batch3", commands3.Keys.Cast<string>().ToArray(), commands3.Values.Cast<string>().ToArray(), "sender3", "endpoint3");
            var batch4 = TerminalInputOutput.Batch("batch4", commands4.Keys.Cast<string>().ToArray(), commands4.Values.Cast<string>().ToArray(), "sender4", "endpoint4");
            var batch5 = TerminalInputOutput.Batch("batch5", commands5.Keys.Cast<string>().ToArray(), commands5.Values.Cast<string>().ToArray(), "sender5", "endpoint5");
            var batch6 = TerminalInputOutput.Batch("batch6", commands6.Keys.Cast<string>().ToArray(), commands6.Values.Cast<string>().ToArray(), "sender6", "endpoint6");
            var batch7 = TerminalInputOutput.Batch("batch7", commands7.Keys.Cast<string>().ToArray(), commands7.Values.Cast<string>().ToArray(), "sender7", "endpoint7");
            var batch8 = TerminalInputOutput.Batch("batch8", commands8.Keys.Cast<string>().ToArray(), commands8.Values.Cast<string>().ToArray(), "sender8", "endpoint8");
            var batch9 = TerminalInputOutput.Batch("batch9", commands9.Keys.Cast<string>().ToArray(), commands9.Values.Cast<string>().ToArray(), "sender9", "endpoint9");
            var batch10 = TerminalInputOutput.Batch("batch10", commands10.Keys.Cast<string>().ToArray(), commands10.Values.Cast<string>().ToArray(), "sender10", "endpoint10");

            // Add all batches asynchronously
            Task addBatch1 = _terminalProcessor.AddAsync(batch1);
            Task addBatch2 = _terminalProcessor.AddAsync(batch2);
            Task addBatch3 = _terminalProcessor.AddAsync(batch3);
            Task addBatch4 = _terminalProcessor.AddAsync(batch4);
            Task addBatch5 = _terminalProcessor.AddAsync(batch5);
            Task addBatch6 = _terminalProcessor.AddAsync(batch6);
            Task addBatch7 = _terminalProcessor.AddAsync(batch7);
            Task addBatch8 = _terminalProcessor.AddAsync(batch8);
            Task addBatch9 = _terminalProcessor.AddAsync(batch9);
            Task addBatch10 = _terminalProcessor.AddAsync(batch10);

            // Wait for all batches to be processed
            await Task.WhenAll(addBatch1, addBatch2, addBatch3, addBatch4, addBatch5, addBatch6, addBatch7, addBatch8, addBatch9, addBatch10);

            // Stop the processing with a timeout of 5000ms
            await _terminalProcessor.StopProcessingAsync(5000);

            // Verify all commands are processed
            routedCommands.Should().HaveCount(10000);
            _terminalProcessor.UnprocessedIOs.Should().HaveCount(0);

            // Collect the processed commands into groups by their prefixes (e.g., "command_1_", "command_2_", etc.)
            Dictionary<string, List<string>> groupedCommands = routedCommands.GroupBy(r => r.Split('_')[1])
                .ToDictionary(g => g.Key, g => g.Select(r => r).ToList());

            // Verify grouped commands
            groupedCommands["1"].Should().BeEquivalentTo(commands1.Values.Cast<string>().ToList(), options => options.WithStrictOrdering());
            groupedCommands["2"].Should().BeEquivalentTo(commands2.Values.Cast<string>().ToList(), options => options.WithStrictOrdering());
            groupedCommands["3"].Should().BeEquivalentTo(commands3.Values.Cast<string>().ToList(), options => options.WithStrictOrdering());
            groupedCommands["4"].Should().BeEquivalentTo(commands4.Values.Cast<string>().ToList(), options => options.WithStrictOrdering());
            groupedCommands["5"].Should().BeEquivalentTo(commands5.Values.Cast<string>().ToList(), options => options.WithStrictOrdering());
            groupedCommands["6"].Should().BeEquivalentTo(commands6.Values.Cast<string>().ToList(), options => options.WithStrictOrdering());
            groupedCommands["7"].Should().BeEquivalentTo(commands7.Values.Cast<string>().ToList(), options => options.WithStrictOrdering());
            groupedCommands["8"].Should().BeEquivalentTo(commands8.Values.Cast<string>().ToList(), options => options.WithStrictOrdering());
            groupedCommands["9"].Should().BeEquivalentTo(commands9.Values.Cast<string>().ToList(), options => options.WithStrictOrdering());
            groupedCommands["10"].Should().BeEquivalentTo(commands10.Values.Cast<string>().ToList(), options => options.WithStrictOrdering());
        }

        [Fact]
        public async Task AddAsync_Processes_Large_Batch()
        {
            _mockOptions.Object.Value.Router.MaxLength = 1500000;
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Setup that the mock command router was invoked
            Dictionary<string, string> routedCommands = [];
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandContext>()))
                .Callback<CommandContext>(c => routedCommands.Add(c.Request.Id, c.Request.Raw))
                .ReturnsAsync(new CommandResult());

            // Send batch of 100000 commands by using TerminalServices
            Dictionary<string, string> allCommands = [];
            foreach (var i in Enumerable.Range(0, 100000))
            {
                allCommands.Add($"id{i}", $"command{i}");
            }
            var longBatch = TerminalInputOutput.Batch("batch_id", allCommands.Keys.ToArray(), allCommands.Values.ToArray(), "sender", "endpoint");
            await _terminalProcessor.AddAsync(longBatch);

            await _terminalProcessor.StopProcessingAsync(5000);

            // We are iterating over a large number of unprocessed requests, so we need to ensure that the validation
            // code is not too slow. We are also checking that all commands are present in the batch at the same time
            // reducing the batch size so that the test does not take too long to run.
            routedCommands.Should().HaveCount(100000);
            _terminalProcessor.UnprocessedIOs.Should().HaveCount(0);
            foreach (var request in routedCommands)
            {
                if (allCommands.ContainsValue(request.Value))
                {
                    allCommands.Remove(request.Key);
                }
                else
                {
                    throw new InvalidOperationException($"An unexpected command was added to the unprocessed requests. command={request}");
                }
            }
            allCommands.Keys.Count.Should().Be(0, "All commands are accounted for.");
        }

        [Fact]
        public void ByDefault_Processor_Is_NotProcessing()
        {
            _terminalProcessor.IsProcessing.Should().BeFalse();
            _terminalProcessor.IsBackground.Should().BeFalse();
        }

        [Fact]
        public async Task Execute_Errors_WhenBatchTooLong()
        {
            _mockOptions.Object.Value.Router.MaxLength = 1000;

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            var longRaw = new string('A', 1001);

            TerminalInputOutput terminalIO = TerminalInputOutput.Single("id1", longRaw, "sender", "endpoint");
            await _terminalProcessor.ExecuteAsync(terminalIO);
            terminalIO.Requests[0].Result.Should().BeOfType<Error>();

            Error error = (Error)terminalIO.Requests[0].Result!;
            error.FormatDescription().Should().Be("The command length exceeds the maximum allowed. max=1000");
        }

        [Fact]
        public async Task ExecuteAsync_Routes_Batched_Commands_And_Processes_In_Order()
        {
            TerminalRequest testRequest = new("id1", "command1,command2,command3|");

            // Create mock command router results for each command
            CommandResult routerResult1 = new(new CommandCheckerResult(), new CommandRunnerResult("sender_result1"));
            CommandResult routerResult2 = new(new CommandCheckerResult(), new CommandRunnerResult("sender_result2"));
            CommandResult routerResult3 = new(new CommandCheckerResult(), new CommandRunnerResult("sender_result3"));

            // Arrange
            CommandContext? routeContext = null;
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandContext>()))
                .Callback<CommandContext>(c => routeContext = c)
                .ReturnsAsync((CommandContext context) =>
                {
                    return context.Request.Raw switch
                    {
                        "command1" => routerResult1,
                        "command2" => routerResult2,
                        "command3" => routerResult3,
                        _ => throw new InvalidOperationException("Unexpected command")
                    };
                });

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act
            TerminalInputOutput batch = TerminalInputOutput.Batch("id1", ["id1", "id2", "id3"], ["command1", "command2", "command3"], "sender_1", "sender_endpoint_1");
            await _terminalProcessor.ExecuteAsync(batch);

            // Assert route context and response
            routeContext.Should().NotBeNull();
            routeContext!.Properties.Should().HaveCount(2);
            routeContext.Properties!["sender_endpoint"].Should().Be("sender_endpoint_1");
            routeContext.Properties!["sender_id"].Should().Be("sender_1");
            routeContext.TerminalContext.Should().BeSameAs(_mockTerminalRouterContext.Object);

            batch.Requests.Should().HaveCount(3);

            // Assert first request and result
            batch.Requests[0].Raw.Should().Be("command1");
            batch.Requests[0].Result.Should().Be("sender_result1");

            // Assert second request and result
            batch.Requests[1].Raw.Should().Be("command2");
            batch.Requests[1].Result.Should().Be("sender_result2");

            // Assert third request and result
            batch.Requests[2].Raw.Should().Be("command3");
            batch.Requests[2].Result.Should().Be("sender_result3");
        }

        [Fact]
        public async Task ExecuteAsync_Routes_Batched_Commands_With_Null_Value_Result()
        {
            TerminalRequest testRequest = new("id1", "command1,command2,command3,command4,command5|");

            // Create mock command router results: 4 valid and 1 null
            CommandResult routerResult1 = new(new CommandCheckerResult(), new CommandRunnerResult("sender_result1"));
            CommandResult routerResult2 = new(new CommandCheckerResult(), new CommandRunnerResult("sender_result2"));
            CommandResult routerResult3 = new(new CommandCheckerResult(), new CommandRunnerResult("sender_result3"));
            CommandResult routerResult4 = new(new CommandCheckerResult(), new CommandRunnerResult()); // Null Value
            CommandResult routerResult5 = new(new CommandCheckerResult(), new CommandRunnerResult("sender_result5"));

            // Arrange
            CommandContext? routeContext = null;
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandContext>()))
                .Callback<CommandContext>(c => routeContext = c)
                .ReturnsAsync((CommandContext context) =>
                {
                    return context.Request.Raw switch
                    {
                        "command1" => routerResult1,
                        "command2" => routerResult2,
                        "command3" => routerResult3,
                        "command4" => routerResult4,
                        "command5" => routerResult5,
                        _ => throw new InvalidOperationException("Unexpected command")
                    };
                });

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act
            TerminalInputOutput terminalIO = TerminalInputOutput.Batch("id1", ["id1", "id2", "id3", "id4", "id5"], ["command1", "command2", "command3", "command4", "command5"], "sender_1", "sender_endpoint_1");
            await _terminalProcessor.ExecuteAsync(terminalIO);

            // Assert route context and response
            routeContext.Should().NotBeNull();
            routeContext!.Properties.Should().HaveCount(2);
            routeContext.Properties!["sender_endpoint"].Should().Be("sender_endpoint_1");
            routeContext.Properties!["sender_id"].Should().Be("sender_1");
            routeContext.TerminalContext.Should().BeSameAs(_mockTerminalRouterContext.Object);

            // Assert requests and results
            terminalIO.Requests[0].Raw.Should().Be("command1");
            terminalIO.Requests[0].Result.Should().Be("sender_result1");

            terminalIO.Requests[1].Raw.Should().Be("command2");
            terminalIO.Requests[1].Result.Should().Be("sender_result2");

            terminalIO.Requests[2].Raw.Should().Be("command3");
            terminalIO.Requests[2].Result.Should().Be("sender_result3");

            terminalIO.Requests[3].Raw.Should().Be("command4");
            terminalIO.Requests[3].Result.Should().BeNull(); // Command4 returns null

            terminalIO.Requests[4].Raw.Should().Be("command5");
            terminalIO.Requests[4].Result.Should().Be("sender_result5");
        }

        [Fact]
        public async Task ExecuteAsync_Routes_Command_And_Returns_Result()
        {
            TerminalRequest testRequest = new("id1", "command1|");

            // Create a mock command router result with
            CommandResult routerResult = new(new CommandCheckerResult(), new CommandRunnerResult("sender_result"));

            // Arrange
            CommandContext? routeContext = null;
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandContext>()))
                .Callback<CommandContext>(c => routeContext = c)
                .ReturnsAsync(routerResult);

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act
            TerminalInputOutput terminalIO = TerminalInputOutput.Single("id1", "command1", "sender_1", "sender_endpoint_1");
            await _terminalProcessor.ExecuteAsync(terminalIO);

            // Make sure context is correctly populated
            routeContext.Should().NotBeNull();
            routeContext!.Properties.Should().HaveCount(2);
            routeContext.Properties!["sender_endpoint"].Should().Be("sender_endpoint_1");
            routeContext.Properties!["sender_id"].Should().Be("sender_1");
            routeContext.Request.Id.Should().NotBeNullOrWhiteSpace();
            routeContext.Request.Raw.Should().Be("command1");
            routeContext.TerminalContext.Should().BeSameAs(_mockTerminalRouterContext.Object);

            // Make sure response is correct
            terminalIO.SenderId.Should().Be("sender_1");
            terminalIO.SenderEndpoint.Should().Be("sender_endpoint_1");

            terminalIO.Requests.Should().HaveCount(1);
            terminalIO.Requests[0].Id.Should().NotBeNullOrWhiteSpace();
            terminalIO.Requests[0].Raw.Should().Be("command1");
            terminalIO.Requests[0].Result.Should().Be("sender_result");
        }

        [Fact]
        public async Task ExecuteAsync_Without_Processing_Throws()
        {
            // Act
            TerminalInputOutput terminalIO = TerminalInputOutput.Single("id1", "command1", "sender_1", "sender_endpoint_1");
            Func<Task> act = async () => await _terminalProcessor.ExecuteAsync(terminalIO);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("server_error")
                .WithErrorDescription("The terminal processor is not running.");
        }

        [Fact]
        public async Task StartProcessing_HandlesRouterException()
        {
            Exception? handeledException = null;
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandContext>())).ThrowsAsync(new Exception("Router error"));
            _mockExceptionHandler.Setup(e => e.HandleExceptionAsync(It.IsAny<TerminalExceptionHandlerContext>())).Callback<TerminalExceptionHandlerContext>(c => handeledException = c.Exception);

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);
            await _terminalProcessor.AddAsync(TerminalInputOutput.Single("id", "command1", "sender", "endpoint"));
            await Task.Delay(500);

            handeledException.Should().NotBeNull();
            handeledException!.Message.Should().Be("Router error");
        }

        [Fact]
        public async Task StartProcessing_Populates_Router_Context()
        {
            CommandContext? routeContext = null;
            _mockCommandRouter.Setup(r => r.RouteCommandAsync(It.IsAny<CommandContext>())).Callback<CommandContext>(c => routeContext = c);

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Add with sender endpoint and sender id
            await _terminalProcessor.AddAsync(TerminalInputOutput.Single("id", "command1", "sender_1", "sender_endpoint_1"));
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
            await _terminalProcessor.AddAsync(TerminalInputOutput.Single("id", "command2", null, null));
            await Task.Delay(500);

            routeContext.Should().NotBeNull();
            routeContext!.Properties.Should().HaveCount(2);
            routeContext.Properties!["sender_id"].Should().Be("$unknown$");
            routeContext.Properties!["sender_endpoint"].Should().Be("$unknown$");

            routeContext.Request.Id.Should().NotBeNullOrWhiteSpace();
            routeContext.Request.Raw.Should().Be("command2");

            routeContext.TerminalContext.Should().BeSameAs(_mockTerminalRouterContext.Object);

            await _terminalProcessor.StopProcessingAsync(2000);
        }

        [Fact]
        public void StartProcessing_With_Background_Sets_Fields()
        {
            _terminalProcessor.IsProcessing.Should().BeFalse();
            _terminalProcessor.IsBackground.Should().BeFalse();

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            _terminalProcessor.IsProcessing.Should().BeTrue();
            _terminalProcessor.IsBackground.Should().BeTrue();
            _terminalTokenSource.Cancel();
        }

        [Fact]
        public void StartProcessing_Without_Background_Sets_Fields()
        {
            _terminalProcessor.IsProcessing.Should().BeFalse();
            _terminalProcessor.IsBackground.Should().BeFalse();

            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: false);

            _terminalProcessor.IsProcessing.Should().BeTrue();
            _terminalProcessor.IsBackground.Should().BeFalse();
            _terminalTokenSource.Cancel();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1000)]
        [InlineData(5000)]
        [InlineData(Timeout.Infinite)]
        public async Task StopProcessingAsync_Any_Timeout_And_Completed_Processing_Sets_IsProcessing_ToFalse(int timeout)
        {
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);
            _terminalProcessor.IsProcessing.Should().BeTrue();
            _terminalTokenSource.Cancel();
            await Task.Delay(500);
            var timedOut = await _terminalProcessor.StopProcessingAsync(timeout);
            timedOut.Should().BeFalse();
            _terminalProcessor.IsProcessing.Should().BeFalse();
        }

        [Fact]
        public async Task StopProcessingAsync_Sets_IsProcessing_ToFalse()
        {
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);
            _terminalTokenSource.CancelAfter(300);

            _terminalProcessor.IsProcessing.Should().BeTrue();
            bool timeout = await _terminalProcessor.StopProcessingAsync(Timeout.Infinite);
            _terminalProcessor.IsProcessing.Should().BeFalse();
            timeout.Should().BeFalse();
        }

        [Fact]
        public async Task StopProcessingAsync_Throws_If_Not_Processing()
        {
            _terminalProcessor.IsProcessing.Should().BeFalse();
            Func<Task> act = async () => await _terminalProcessor.StopProcessingAsync(100);
            await act.Should().ThrowAsync<TerminalException>()
                .WithErrorCode("invalid_request")
                .WithErrorDescription("The terminal processor is not running.");
        }

        [Fact]
        public async Task StopProcessingAsync_TimesOut_Return_True_Sets_IsProcessing_ToTrue()
        {
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);
            _terminalTokenSource.CancelAfter(400);
            _terminalProcessor.IsProcessing.Should().BeTrue();
            bool timeout = await _terminalProcessor.StopProcessingAsync(100);
            _terminalProcessor.IsProcessing.Should().BeTrue();
            timeout.Should().BeTrue();

            // give time for the processor to stop
            await Task.Delay(500);
        }

        [Fact]
        public async Task Stream_Does_No_Processes_Partial_Batch()
        {
            // Arrange
            var senderId = "large_data_sender";
            var senderEndpoint = "large_data_endpoint";
            List<string> processedCommands = [];
            var lockObj = new object();

            _mockCommandRouter.Setup(x => x.RouteCommandAsync(It.IsAny<CommandContext>()))
                .Callback<CommandContext>(ctx =>
                {
                    processedCommands.Add(ctx.Request.Raw);
                })
                .ReturnsAsync(new CommandResult());

            // Start the processor
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act: Stream data in chunks to the processor
            TerminalInputOutput input1 = TerminalInputOutput.Single("id1", "command1");
            TerminalInputOutput input2 = TerminalInputOutput.Single("id2", "command2");
            TerminalInputOutput input3 = TerminalInputOutput.Single("id3", "command3");

            // Ensure batch ends with delimiter
            var bytes1 = TerminalServices.DelimitBytes(JsonSerializer.SerializeToUtf8Bytes(input1), _mockOptions.Object.Value.Router.StreamDelimiter);
            var bytes2 = TerminalServices.DelimitBytes(JsonSerializer.SerializeToUtf8Bytes(input2), _mockOptions.Object.Value.Router.StreamDelimiter);
            var bytes3NonDelimited = JsonSerializer.SerializeToUtf8Bytes(input2);

            // The last batch is not delimited.
            byte[] bytes = bytes1.Concat(bytes2).Concat(bytes3NonDelimited).ToArray();
            await _terminalProcessor.StreamAsync(bytes, bytes.Length, senderId, senderEndpoint);

            // Allow time for processing to complete
            await Task.Delay(100);

            // Assert: Verify command was are processed since the batch is partial
            processedCommands.Should().HaveCount(2);
            processedCommands.Should().BeEquivalentTo(["command1", "command2"]);

            // Ensure there are not unprocessed requests since the batch is partial
            _terminalProcessor.UnprocessedIOs.Should().HaveCount(0);
        }

        [Fact]
        public async Task Stream_Does_Not_Process_Non_Delimited_Input()
        {
            // Arrange
            var senderId = "large_data_sender";
            var senderEndpoint = "large_data_endpoint";
            string? processedCommand = null;
            var lockObj = new object();

            _mockCommandRouter.Setup(x => x.RouteCommandAsync(It.IsAny<CommandContext>()))
                .Callback<CommandContext>(ctx =>
                {
                    processedCommand = ctx.Request.Raw;
                });

            // Start the processor
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act: Stream data in chunks to the processor
            TerminalInputOutput terminalInput = TerminalInputOutput.Single("id1", "command1");
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(terminalInput);
            await _terminalProcessor.StreamAsync(bytes, bytes.Length, senderId, senderEndpoint);

            // Allow time for processing to complete
            await Task.Delay(100);

            // Assert: Verify command was not processed since the batch is incomplete
            processedCommand.Should().BeNull();

            // Ensure there are not unprocessed requests since the batch is incomplete
            _terminalProcessor.UnprocessedIOs.Should().HaveCount(0);
        }

        [Fact]
        public async Task Stream_Processes_Chunks_In_Order()
        {
            // Arrange
            var senderId = "large_data_sender";
            var senderEndpoint = "large_data_endpoint";
            var processedCommands = new ConcurrentQueue<string>();
            var lockObj = new object();

            _mockCommandRouter.Setup(x => x.RouteCommandAsync(It.IsAny<CommandContext>()))
                .Callback<CommandContext>(ctx =>
                {
                    processedCommands.Enqueue(ctx.Request.Raw);
                })
                .ReturnsAsync(new CommandResult());

            // Create a large batch of commands to simulate streaming
            var ids = Enumerable.Range(1, 1500).Select(i => $"id{i}").ToArray();
            var commands = Enumerable.Range(1, 1500).Select(i => $"command{i}").ToArray();
            TerminalInputOutput batch = TerminalInputOutput.Batch("batch1", ids, commands);

            // Ensure batch ends with delimiter
            var completeBatchBytes = JsonSerializer.SerializeToUtf8Bytes(batch);
            var delimitedCompleteBatchBytes = TerminalServices.DelimitBytes(completeBatchBytes, _mockOptions.Object.Value.Router.StreamDelimiter);

            // Split the data into sizable chunks to simulate streaming
            int chunkSize = 30;
            var chunks = delimitedCompleteBatchBytes.Chunk(chunkSize).ToArray();

            // Start the processor
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Act: Stream data in chunks to the processor
            foreach (var chunk in chunks)
            {
                await _terminalProcessor.StreamAsync(chunk, chunk.Length, senderId, senderEndpoint);
            }

            // Allow time for processing to complete
            await Task.Delay(3000);

            // Assert: Verify all commands were processed in the correct order
            processedCommands.ToArray().Should().BeEquivalentTo(commands);

            // Ensure no unprocessed requests are left
            _terminalProcessor.UnprocessedIOs.Should().BeEmpty();
        }

        [Fact]
        public async Task StreamRequestAsync_Processes_Delimited_Input()
        {
            // Arrange
            var senderId = "large_data_sender";
            var senderEndpoint = "large_data_endpoint";
            var processedCommand = "";
            var lockObj = new object();

            _mockCommandRouter.Setup(x => x.RouteCommandAsync(It.IsAny<CommandContext>()))
                .Callback<CommandContext>(ctx =>
                {
                    processedCommand = ctx.Request.Raw;
                });

            // Start the processor
            _terminalProcessor.StartProcessing(_mockTerminalRouterContext.Object, background: true);

            // Delimit the input
            TerminalInputOutput terminalInput = TerminalInputOutput.Single("id1", "command1");
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(terminalInput);
            byte[] delimitedBytes = TerminalServices.DelimitBytes(bytes, _mockOptions.Object.Value.Router.StreamDelimiter);
            await _terminalProcessor.StreamAsync(delimitedBytes, delimitedBytes.Length, senderId, senderEndpoint);

            // Allow time for processing to complete
            await Task.Delay(100);

            // Assert: Verify all commands were processed in the correct order
            processedCommand.Should().Be("command1");

            // Ensure no unprocessed requests are left
            _terminalProcessor.UnprocessedIOs.Should().BeEmpty();
        }

        [Fact]
        public async Task WaitAsync_CancelsIndefiniteProcessing()
        {
            // Start the WaitAsync task
            Task waiting = _terminalProcessor.WaitUntilCanceledAsync(_terminalTokenSource.Token);

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
        private readonly TerminalProcessor _terminalProcessor;
        private readonly CancellationTokenSource _terminalTokenSource;
        private readonly ITerminalTextHandler _textHandler;
    }
}
