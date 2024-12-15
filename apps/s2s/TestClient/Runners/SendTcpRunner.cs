using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Client.Extensions;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Runtime;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    [CommandOwners("send")]
    [CommandDescriptor("tcp", "Send TCP", "Send TCP commands to the server.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
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
            try
            {
                stopwatch.Restart();

                string server = configuration["testclient:testserver:ip"] ?? throw new InvalidOperationException("Server IP address is missing.");
                int port = configuration.GetValue<int>("testclient:testserver:port");

                await terminalConsole.WriteLineColorAsync(ConsoleColor.Blue, "TCP concurrent and asynchronous demo");

                var clientTasks = new Task[5];
                for (int idx = 0; idx < clientTasks.Length; idx++)
                {
                    clientTasks[idx] = StartClientAsync(server, port, idx, context.TerminalContext.StartContext.TerminalCancellationToken);
                }

                await Task.WhenAll(clientTasks);

                return new CommandRunnerResult();
            }
            finally
            {
                stopwatch.Stop();
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Green, $"TCP client tasks completed in {stopwatch.Elapsed.TotalSeconds} seconds.");
            }
        }

        private async Task SendCommandsAsync(TcpClient tcpClient, int clientIndex, CancellationToken cToken)
        {
            string[] cmdIds = ["cmd1", "cmd2", "cmd3", "cmd4", "cmd5", "cmd6"];
            string[] commands = ["ts", "ts -v", "ts grp1", "ts grp1 cmd1", "ts grp1 grp2", "ts grp1 grp2 cmd2"];

            for (int idx = 0; idx < commands.Length; ++idx)
            {
                string id = cmdIds[idx];
                string raw = commands[idx];

                TerminalInput single = TerminalInput.Single(id, raw);
                await tcpClient.SendToTerminalAsync(single, terminalOptions.Value.Router.StreamDelimiter, cToken);
                await terminalConsole.WriteLineAsync($"[Client {clientIndex}] Request=\"{id}\" Raw=\"{raw}\" => Sent");
            }

            string batchId = $"batch{clientIndex}";
            TerminalInput batch = TerminalInput.Batch(batchId, cmdIds, commands);
            await tcpClient.SendToTerminalAsync(batch, terminalOptions.Value.Router.StreamDelimiter, cToken);
            await terminalConsole.WriteLineAsync($"[Client {clientIndex}] BatchId=\"{batchId}\" => Batch Sent");

            int processedRequests = 0;
            int expectedRequests = 12; // 6 Individual commands + 6 commands from Batch
            try
            {
                using NetworkStream stream = tcpClient.GetStream();
                while (tcpClient.Connected)
                {
                    if (processedRequests == expectedRequests)
                    {
                        break;
                    }

                    byte[] buffer = new byte[4096];
                    int bytesRead = await stream.ReadAsync(buffer, cToken);

                    if (bytesRead > 0)
                    {
                        var outputs = buffer.Take(bytesRead).ToArray().Split(terminalOptions.Value.Router.StreamDelimiter, ignoreEmpty: true, out _);
                        foreach (var output in outputs)
                        {
                            TerminalOutput? response = JsonSerializer.Deserialize<TerminalOutput>(output);

                            for (int idx = 0; idx < response!.Input.Count; ++idx)
                            {
                                if (response.Input.IsBatch)
                                {
                                    await terminalConsole.WriteLineAsync($"[Client {clientIndex}] Response: BatchId=\"{response.Input.BatchId}\" Request=\"{response.Input[idx].Id}\" => Result={response.Results[idx]}");
                                }
                                else
                                {
                                    await terminalConsole.WriteLineAsync($"[Client {clientIndex}] Response: Request=\"{response.Input[idx].Id}\" => Result={response.Results[idx]}");
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
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Blue, $"[Client {clientIndex}] Streaming status: Expected Requests={expectedRequests} Actual Requests={processedRequests}");
            }
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

                await SendCommandsAsync(tcpClient, clientIndex, cToken);
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
    }
}
