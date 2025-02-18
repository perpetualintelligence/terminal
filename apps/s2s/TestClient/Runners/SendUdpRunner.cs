using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OneImlx.Shared.Infrastructure;
using OneImlx.Terminal.Client.Extensions;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    [CommandOwners("send")]
    [CommandDescriptor("udp", "UDP test", "Send UDP commands to the terminal server.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    public class SendUdpRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public SendUdpRunner(
            IOptions<TerminalOptions> terminalOptions,
            ITerminalTextHandler terminalTextHandler,
            ITerminalConsole terminalConsole,
            IConfiguration configuration,
            ITerminalExceptionHandler terminalExceptionHandler)
        {
            this.terminalOptions = terminalOptions;
            this.terminalTextHandler = terminalTextHandler;
            this.terminalConsole = terminalConsole;
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.terminalExceptionHandler = terminalExceptionHandler;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            string server = configuration.GetValue<string>("testclient:testserver:ip")
                            ?? throw new InvalidOperationException("Server IP address is missing.");
            int port = configuration.GetValue<int?>("testclient:testserver:port")
                           ?? throw new InvalidOperationException("Server port is missing or invalid.");
            int maxClients = configuration.GetValue<int>("testclient:max_clients");

            try
            {
                stopwatch.Restart();

                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "UDP concurrent and asynchronous demo");

                var clientTasks = new Task[maxClients];
                for (int idx = 0; idx < clientTasks.Length; idx++)
                {
                    clientTasks[idx] = StartClientAsync(server, port, idx, context.TerminalContext.TerminalCancellationToken);
                }

                await Task.WhenAll(clientTasks);
                return new CommandRunnerResult();
            }
            finally
            {
                stopwatch.Stop();
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Green, $"{maxClients * 14} requests completed by {maxClients} UDP client tasks in {stopwatch.Elapsed.TotalMilliseconds} milliseconds.");
            }
        }

        private async Task ReceiveResponsesAsync(UdpClient udpClient, int clientIndex)
        {
            int processedRequests = 0;
            int expectedRequests = 14; // 6 Individual commands + 6 commands from Batch
            try
            {
                while (true)
                {
                    if (processedRequests == expectedRequests)
                    {
                        break;
                    }

                    var udpResult = await udpClient.ReceiveAsync();
                    var outputs = udpResult.Buffer.Split(terminalOptions.Value.Router.StreamDelimiter, ignoreEmpty: true, out _);
                    foreach (var opt in outputs)
                    {
                        TerminalInputOutput? output = JsonSerializer.Deserialize<TerminalInputOutput>(opt);
                        if (output == null)
                        {
                            continue;
                        }

                        for (int idx = 0; idx < output.Count; ++idx)
                        {
                            var request = output[idx];
                            object? result = request.Result;
                            string resultStr = result?.ToString() ?? "No Result";

                            if (output.IsBatch)
                            {
                                if (request.IsError)
                                {
                                    Error error = output.GetDeserializedResult<Error>(idx);
                                    resultStr = error.FormatDescription();
                                    await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] BatchId=\"{output.BatchId}\" Request=\"{request.Id}\" Raw=\"{request.Raw}\" => Result={resultStr}");
                                }
                                else
                                {
                                    await terminalConsole.WriteLineAsync($"[Client {clientIndex}] Response: BatchId=\"{output.BatchId}\" Request=\"{output[idx].Id}\" => Result={resultStr}");
                                }
                            }
                            else
                            {
                                if (request.IsError)
                                {
                                    Error error = output.GetDeserializedResult<Error>(idx);
                                    resultStr = error.FormatDescription();
                                    await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] Request=\"{request.Id}\" Raw=\"{request.Raw}\" => Result={resultStr}");
                                }
                                else
                                {
                                    await terminalConsole.WriteLineAsync($"[Client {clientIndex}] Response: Request=\"{output[idx].Id}\" => Result={resultStr}");
                                }
                            }

                            processedRequests++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await terminalExceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex));
            }
            finally
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Magenta, $"[Client {clientIndex}] Streaming status: Expected Requests={expectedRequests} Actual Requests={processedRequests}");
            }
        }

        private async Task SendCommandsAsync(UdpClient udpClient, IPEndPoint remoteEndPoint, int clientIndex, CancellationToken cToken)
        {
            string[] cmdIds = ["cmd1", "cmd2", "cmd3", "cmd4", "cmd5", "cmd6", "cmd7"];
            string[] commands = ["ts", "ts -v", "ts grp1", "ts grp1 cmd1", "ts grp1 grp2", "ts grp1 grp2 cmd2", "invalid"];

            foreach (var (cmdId, command) in cmdIds.Zip(commands))
            {
                TerminalInputOutput single = TerminalInputOutput.Single(cmdId, command);
                await udpClient.SendToTerminalAsync(single, TerminalIdentifiers.StreamDelimiter, remoteEndPoint, cToken);

                await terminalConsole.WriteLineAsync($"[Client {clientIndex}] Request=\"{cmdId}\" Raw=\"{command}\" => Sent");
            }

            string batchId = $"batch{clientIndex}";
            TerminalInputOutput batch = TerminalInputOutput.Batch(batchId, cmdIds, commands);
            await udpClient.SendToTerminalAsync(batch, TerminalIdentifiers.StreamDelimiter, remoteEndPoint, cToken);

            await terminalConsole.WriteLineAsync($"[Client {clientIndex}] BatchId=\"{batchId}\" => Batch Sent");
        }

        private async Task StartClientAsync(string server, int port, int clientIndex, CancellationToken cToken)
        {
            using var udpClient = new UdpClient();
            IPEndPoint remoteEndPoint = new(IPAddress.Parse(server), port);

            try
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Magenta, $"UDP client {clientIndex} initialized for {server}:{port}...");

                Task sendTask = SendCommandsAsync(udpClient, remoteEndPoint, clientIndex, cToken);
                Task receiveTask = ReceiveResponsesAsync(udpClient, clientIndex);
                await Task.WhenAll(sendTask, receiveTask);
            }
            catch (Exception ex)
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] Error: {ex.Message}");
            }
            finally
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Green, $"[Client {clientIndex}] Connection closed.");
            }
        }

        private readonly IConfiguration configuration;
        private readonly Stopwatch stopwatch = new();
        private readonly ITerminalConsole terminalConsole;
        private readonly ITerminalExceptionHandler terminalExceptionHandler;
        private readonly IOptions<TerminalOptions> terminalOptions;
        private readonly ITerminalTextHandler terminalTextHandler;
    }
}
