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
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    [CommandOwners("send")]
    [CommandDescriptor("tcp", "TCP test", "Send TCP commands to the terminal server.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    public class SendTcpRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public SendTcpRunner(IOptions<TerminalOptions> terminalOptions,
                             ITerminalTextHandler terminalTextHandler,
                             ITerminalConsole terminalConsole,
                             IConfiguration configuration,
                             ITerminalExceptionHandler terminalExceptionHandler)
        {
            this.terminalOptions = terminalOptions ?? throw new ArgumentNullException(nameof(terminalOptions));
            this.terminalTextHandler = terminalTextHandler;
            this.terminalConsole = terminalConsole;
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.terminalExceptionHandler = terminalExceptionHandler;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            string server = configuration["testclient:testserver:ip"] ?? throw new InvalidOperationException("Server IP address is missing.");
            int port = configuration.GetValue<int>("testclient:testserver:port");
            int maxClients = configuration.GetValue<int>("testclient:max_clients");

            try
            {
                stopwatch.Restart();
                _commandCount = 0;

                await terminalConsole.WriteLineColorAsync(ConsoleColor.Magenta, "TCP concurrent and asynchronous demo");

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
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Green, $"Completed {maxClients * _commandCount} requests from {maxClients} TCP client tasks in {stopwatch.Elapsed.TotalMilliseconds} milliseconds.");
            }
        }

        private async Task ReceiveResponsesAsync(TcpClient tcpClient, int clientIndex, CancellationToken cToken)
        {
            int processedRequests = 0;
            try
            {
                using NetworkStream stream = tcpClient.GetStream();
                while (tcpClient.Connected)
                {
                    if (processedRequests == _commandCount)
                    {
                        break;
                    }

                    byte[] buffer = new byte[4096];
                    int bytesRead = await stream.ReadAsync(buffer, cToken);

                    if (bytesRead > 0)
                    {
                        byte[][] outputs = buffer.Take(bytesRead).ToArray().Split(terminalOptions.Value.Router.StreamDelimiter, ignoreEmpty: true, out _);
                        foreach (byte[] opt in outputs)
                        {
                            TerminalInputOutput? output = JsonSerializer.Deserialize<TerminalInputOutput>(opt);
                            if (output == null)
                            {
                                continue;
                            }

                            for (int idx = 0; idx < output.Count; ++idx)
                            {
                                var request = output.Requests[idx];
                                object? result = output.Requests[idx].Result;
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
                    else
                    {
                        await terminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "[Client {clientIndex}] No more data available. Closing client...");
                    }
                }
            }
            catch (Exception ex)
            {
                await terminalExceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex));
            }
            finally
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Blue, $"[Client {clientIndex}] Streaming status: Expected={_commandCount} Processed={processedRequests}");
            }
        }

        private async Task SendCommandsAsync(TcpClient tcpClient, int clientIndex, CancellationToken cToken)
        {
            string[] cmdIds = ["cmd1", "cmd2", "cmd3", "cmd4", "cmd5", "cmd6", "cmd7"];
            string[] commands = ["ts", "ts -v", "ts grp1", "ts grp1 cmd1", "ts grp1 grp2", "ts grp1 grp2 cmd2", "ts invalid"];

            // Single and bulk commands (2) * 7 = 14
            _commandCount = commands.Length * 2;

            for (int idx = 0; idx < commands.Length; ++idx)
            {
                string id = cmdIds[idx];
                string raw = commands[idx];

                TerminalInputOutput single = TerminalInputOutput.Single(id, raw);
                await tcpClient.SendToTerminalAsync(single, terminalOptions.Value.Router.StreamDelimiter, cToken);
                await terminalConsole.WriteLineAsync($"[Client {clientIndex}] Request=\"{id}\" Raw=\"{raw}\" => Sent");
            }

            string batchId = $"batch{clientIndex}";
            TerminalInputOutput batch = TerminalInputOutput.Batch(batchId, cmdIds, commands);
            await tcpClient.SendToTerminalAsync(batch, terminalOptions.Value.Router.StreamDelimiter, cToken);
            await terminalConsole.WriteLineAsync($"[Client {clientIndex}] BatchId=\"{batchId}\" => Batch Sent");
        }

        private async Task StartClientAsync(string server, int port, int clientIndex, CancellationToken cToken)
        {
            var tcpClient = new TcpClient();

            try
            {
                while (true)
                {
                    try
                    {
                        await tcpClient.ConnectAsync(IPAddress.Parse(server), port);
                        await terminalConsole.WriteLineColorAsync(ConsoleColor.Magenta, $"TCP client {clientIndex} connected to {tcpClient.Client.RemoteEndPoint}.");
                        break;
                    }
                    catch (SocketException)
                    {
                        await terminalConsole.WriteLineColorAsync(ConsoleColor.DarkMagenta, "[Client {clientIndex}] Server not available, retrying in 5 seconds...");
                        await Task.Delay(5000, cToken);
                    }
                }

                Task sendTask = SendCommandsAsync(tcpClient, clientIndex, cToken);
                Task receiveTask = ReceiveResponsesAsync(tcpClient, clientIndex, cToken);
                await Task.WhenAll(sendTask, receiveTask);
            }
            catch (Exception ex)
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] Error: {ex.Message}");
            }
            finally
            {
                tcpClient.Close();
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Magenta, $"[Client {clientIndex}] Connection closed.");
            }
        }

        private readonly IConfiguration configuration;
        private readonly Stopwatch stopwatch = new();
        private readonly ITerminalConsole terminalConsole;
        private readonly ITerminalExceptionHandler terminalExceptionHandler;
        private readonly IOptions<TerminalOptions> terminalOptions;
        private readonly ITerminalTextHandler terminalTextHandler;
        private int _commandCount;
    }
}
