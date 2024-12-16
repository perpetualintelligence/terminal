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
            try
            {
                stopwatch.Restart();
                string server = configuration.GetValue<string>("testclient:testserver:ip")
                           ?? throw new InvalidOperationException("Server IP address is missing.");
                int port = configuration.GetValue<int?>("testclient:testserver:port")
                               ?? throw new InvalidOperationException("Server port is missing or invalid.");

                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "UDP concurrent and asynchronous demo");

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
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Green, $"UDP client tasks completed in {stopwatch.Elapsed.TotalMilliseconds} milliseconds.");
            }
        }

        private async Task ReceiveResponsesAsync(UdpClient udpClient, int clientIndex)
        {
            int processedRequests = 0;
            int expectedRequests = 12; // 6 Individual commands + 6 commands from Batch
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
            string[] cmdIds = ["cmd1", "cmd2", "cmd3", "cmd4", "cmd5", "cmd6"];
            string[] commands = ["ts", "ts -v", "ts grp1", "ts grp1 cmd1", "ts grp1 grp2", "ts grp1 grp2 cmd2"];

            foreach (var (cmdId, command) in cmdIds.Zip(commands))
            {
                TerminalInput single = TerminalInput.Single(cmdId, command);
                await udpClient.SendToTerminalAsync(single, TerminalIdentifiers.StreamDelimiter, remoteEndPoint, cToken);

                await terminalConsole.WriteLineAsync($"[Client {clientIndex}] Request=\"{cmdId}\" Raw=\"{command}\" => Sent");
            }

            string batchId = $"batch{clientIndex}";
            TerminalInput batch = TerminalInput.Batch(batchId, cmdIds, commands);
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
