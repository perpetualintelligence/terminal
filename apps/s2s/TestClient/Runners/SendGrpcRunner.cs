using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Grpc.Core;
using Grpc.Net.Client;
using OneImlx.Shared.Infrastructure;
using OneImlx.Terminal.Client;
using OneImlx.Terminal.Client.Extensions;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    [CommandOwners("send")]
    [CommandDescriptor("grpc", "gRPC test", "Send gRPC commands to the terminal server.", CommandType.SubCommand, CommandFlags.None)]
    public class SendGrpcRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public SendGrpcRunner(IConfiguration configuration, ITerminalConsole terminalConsole)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.terminalConsole = terminalConsole ?? throw new ArgumentNullException(nameof(terminalConsole));
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            string ip = configuration["testclient:testserver:ip"] ?? throw new InvalidOperationException("Server IP address is missing.");
            string port = configuration.GetValue<string>("testclient:testserver:port") ?? throw new InvalidOperationException("Server port is missing.");
            string serverAddress = $"http://{ip}:{port}";
            int maxClients = configuration.GetValue<int>("testclient:max_clients");

            try
            {
                stopwatch.Restart();
                _commandCount = 0;

                await terminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "gRPC concurrent and asynchronous demo.");

                var clientTasks = new Task[maxClients];
                for (int idx = 0; idx < clientTasks.Length; idx++)
                {
                    clientTasks[idx] = StartClientAsync(serverAddress, idx, context.TerminalContext.TerminalCancellationToken);
                }

                await Task.WhenAll(clientTasks);
                return new CommandRunnerResult();
            }
            finally
            {
                stopwatch.Stop();
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Green, $"{_commandCount} requests completed by {maxClients} gRPC client tasks in {stopwatch.Elapsed.TotalMilliseconds} milliseconds.");
            }
        }

        private async Task SendCommandsAsync(TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient client, int clientIndex, CancellationToken cToken)
        {
            string[] cmdIds = ["cmd1", "cmd2", "cmd3", "cmd4", "cmd5", "cmd6", "cmd7"];
            string[] commands = ["ts", "ts -v", "ts grp1", "ts grp1 cmd1", "ts grp1 grp2", "ts grp1 grp2 cmd2", "ts invalid"];

            try
            {
                // Send individually
                foreach (var (cmdId, command) in cmdIds.Zip(commands))
                {
                    TerminalInputOutput single = TerminalInputOutput.Single(cmdId, command);
                    TerminalGrpcRouterProtoOutput response = await client.SendToTerminalAsync(single, cToken);
                    TerminalInputOutput? output = JsonSerializer.Deserialize<TerminalInputOutput>(response.OutputJson);

                    if (output == null)
                    {
                        await terminalConsole.WriteLineAsync($"[Client {clientIndex}] Request=\"{cmdId}\" Raw=\"{command}\" => No Response");
                        continue;
                    }

                    string result = output.Requests[0].Result?.ToString() ?? "No Result";
                    if (output.Requests[0].IsError)
                    {
                        Error error = output.GetDeserializedResult<Error>(0);
                        result = error.FormatDescription();
                        await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] Request=\"{cmdId}\" Raw=\"{command}\" => Result={result}");
                    }
                    else
                    {
                        await terminalConsole.WriteLineAsync($"[Client {clientIndex}] Request=\"{cmdId}\" Raw=\"{command}\" => Result={result}");
                    }

                    _commandCount++;
                }

                // Send as a batch
                string batchId = $"batch{clientIndex}";
                TerminalInputOutput batch = TerminalInputOutput.Batch(batchId, cmdIds, commands);
                TerminalGrpcRouterProtoOutput batchResponse = await client.SendToTerminalAsync(batch, cToken);
                TerminalInputOutput? batchOutput = JsonSerializer.Deserialize<TerminalInputOutput>(batchResponse.OutputJson);

                for (int idx = 0; idx < batch.Requests.Length; ++idx)
                {
                    var request = batch.Requests[idx];
                    if (batchOutput == null)
                    {
                        await terminalConsole.WriteLineAsync($"[Client {clientIndex}] BatchId=\"{batchId}\" Request=\"{request.Id}\" Raw=\"{request.Raw}\" => Result={"No Response"}");
                        continue;
                    }

                    string result = batchOutput.Requests[idx].Result?.ToString() ?? "No Result";
                    if (batchOutput.Requests[idx].IsError)
                    {
                        Error error = batchOutput.GetDeserializedResult<Error>(idx);
                        result = error.FormatDescription();
                        await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] BatchId=\"{batchId}\" Request=\"{request.Id}\" Raw=\"{request.Raw}\" => Result={result ?? "No Response"}");
                    }
                    else
                    {
                        await terminalConsole.WriteLineAsync($"[Client {clientIndex}] BatchId=\"{batchId}\" Request=\"{request.Id}\" Raw=\"{request.Raw}\" => Result={result ?? "No Response"}");
                    }

                    _commandCount++;
                }
            }
            catch (RpcException ex)
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] gRPC Error: {ex.Status.StatusCode} {ex.Message}");
            }
            catch (Exception ex)
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] Error: {ex.Message}");
            }
        }

        private async Task StartClientAsync(string serverAddress, int clientIndex, CancellationToken cToken)
        {
            using var channel = GrpcChannel.ForAddress(serverAddress);
            var client = new TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient(channel);

            try
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, $"gRPC client {clientIndex} initialized for {serverAddress}...");
                await SendCommandsAsync(client, clientIndex, cToken);
            }
            catch (Exception ex)
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] {ex.Message}");
            }
            finally
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, $"gRPC client {clientIndex} disposed.");
            }
        }

        private readonly IConfiguration configuration;
        private readonly Stopwatch stopwatch = new();
        private readonly ITerminalConsole terminalConsole;
        private int _commandCount;
    }
}
