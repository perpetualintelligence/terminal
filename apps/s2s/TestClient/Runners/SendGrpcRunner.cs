using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using OneImlx.Terminal.Client;
using OneImlx.Terminal.Client.Extensions;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    [CommandOwners("send")]
    [CommandDescriptor("grpc", "Send gRPC", "Send gRPC commands to the server.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    public class SendGrpcRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public SendGrpcRunner(IConfiguration configuration, ITerminalConsole terminalConsole)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.terminalConsole = terminalConsole ?? throw new ArgumentNullException(nameof(terminalConsole));
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            try
            {
                stopwatch.Restart();
                string ip = configuration["testclient:testserver:ip"] ?? throw new InvalidOperationException("Server IP address is missing.");
                int port = configuration.GetValue<int>("testclient:testserver:port");
                if (port == 0)
                {
                    throw new InvalidOperationException("Server port is missing.");
                }
                string serverAddress = $"http://{ip}:{port}";

                await terminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "gRPC concurrent and asynchronous demo.");

                var clientTasks = new Task[5];
                for (int idx = 0; idx < clientTasks.Length; idx++)
                {
                    clientTasks[idx] = StartClientAsync(serverAddress, idx, context.TerminalContext.StartContext.TerminalCancellationToken);
                }

                await Task.WhenAll(clientTasks);
                return new CommandRunnerResult();
            }
            finally
            {
                stopwatch.Stop();
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Green, $"gRPC client tasks completed in {stopwatch.Elapsed.TotalMilliseconds} milliseconds.");
            }
        }

        private async Task SendCommandsAsync(TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient client, int clientIndex, CancellationToken cToken)
        {
            string[] cmdIds = ["cmd1", "cmd2", "cmd3", "cmd4", "cmd5", "cmd6"];
            string[] commands = ["ts", "ts -v", "ts grp1", "ts grp1 cmd1", "ts grp1 grp2", "ts grp1 grp2 cmd2"];

            try
            {
                foreach (var (cmdId, command) in cmdIds.Zip(commands))
                {
                    TerminalInput single = TerminalInput.Single(cmdId, command);
                    var response = await client.SendToTerminalAsync(single, cToken);
                    var output = JsonSerializer.Deserialize<TerminalOutput>(response.OutputJson);

                    await terminalConsole.WriteLineAsync($"[Client {clientIndex}] Request=\"{cmdId}\" Raw=\"{command}\" => Result={output?.Results[0] ?? "No Response"}");
                }

                string batchId = $"batch{clientIndex}";
                TerminalInput batch = TerminalInput.Batch(batchId, cmdIds, commands);
                var batchResponse = await client.SendToTerminalAsync(batch, cToken);
                var batchOutput = JsonSerializer.Deserialize<TerminalOutput>(batchResponse.OutputJson);

                for (int idx = 0; idx < batchOutput!.Input.Requests.Length; ++idx)
                {
                    var request = batchOutput.Input.Requests[idx];
                    var result = batchOutput.Results[idx];
                    await terminalConsole.WriteLineAsync($"[Client {clientIndex}] BatchId=\"{batchId}\" Request=\"{request.Id}\" Raw=\"{request.Raw}\" => Result={result ?? "No Response"}");
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
    }
}
