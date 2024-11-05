using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Grpc.Core;
using Grpc.Net.Client;
using OneImlx.Terminal.Client;
using OneImlx.Terminal.Client.Extensions;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    [CommandOwners("send")]
    [CommandDescriptor("grpc", "Send gRPC", "Send gRPC commands to the server.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    public class SendGrpcRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public SendGrpcRunner(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            string ip = configuration.GetValue<string>("testclient:testserver:ip")
                        ?? throw new InvalidOperationException("The gRPC server IP address is missing.");
            string port = configuration.GetValue<string>("testclient:testserver:port")
                          ?? throw new InvalidOperationException("The gRPC server port is missing.");
            string serverTemplate = "http://{0}:{1}";
            string serverAddress = string.Format(serverTemplate, ip, port);

            Task[] clientTasks = new Task[4];
            for (int i = 0; i < clientTasks.Length; i++)
            {
                clientTasks[i] = StartGrpcClientAsync(serverAddress, context.StartContext.TerminalCancellationToken);
            }

            await Task.WhenAll(clientTasks);
            Console.WriteLine("All gRPC client tasks completed.");
            return CommandRunnerResult.NoProcessing;
        }

        private async Task SendGrpcCommandsAsync(TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient client, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("Sending individual gRPC commands...");

                string[] commands =
                [
                    "ts", "ts -v", "ts grp1", "ts grp1 cmd1", "ts grp1 grp2", "ts grp1 grp2 cmd2"
                ];

                foreach (var command in commands)
                {
                    TerminalGrpcRouterProtoOutput response = await client.SendSingleAsync(command, TerminalIdentifiers.RemoteCommandDelimiter, TerminalIdentifiers.RemoteBatchDelimiter, cancellationToken: cancellationToken);
                    Console.WriteLine($"Sent command: {command}, Response: {response}");
                }

                // Sending commands as a batch
                Console.WriteLine("Sending commands as a batch...");
                TerminalGrpcRouterProtoOutput batchResponse = await client.SendBatchAsync(commands, TerminalIdentifiers.RemoteCommandDelimiter, TerminalIdentifiers.RemoteBatchDelimiter, cancellationToken: cancellationToken);
                Console.WriteLine($"Batch sent. Response: {batchResponse}");
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"gRPC Error: {ex.Status.StatusCode} - {ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task StartGrpcClientAsync(string serverAddress, CancellationToken cancellationToken)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(serverAddress);
                TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient client = new TerminalGrpcRouterProto.TerminalGrpcRouterProtoClient(channel);

                await SendGrpcCommandsAsync(client, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during gRPC client operation: {ex.Message}");
            }
        }

        private readonly IConfiguration configuration;
    }
}
