using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Client.Extensions;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    [CommandOwners("send")]
    [CommandDescriptor("udp", "Send UDP", "Send UDP commands to the server.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    public class SendUdpRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public SendUdpRunner(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            string server = configuration.GetValue<string>("testclient:testserver:ip")
                            ?? throw new InvalidOperationException("Server IP address is missing.");
            int port = configuration.GetValue<int?>("testclient:testserver:port")
                           ?? throw new InvalidOperationException("Server port is missing or invalid.");

            Task[] clientTasks =
            [
                StartClientAsync(server, port, context.StartContext.TerminalCancellationToken),
                StartClientAsync(server, port, context.StartContext.TerminalCancellationToken),
                StartClientAsync(server, port, context.StartContext.TerminalCancellationToken),
                StartClientAsync(server, port, context.StartContext.TerminalCancellationToken)
            ];

            await Task.WhenAll(clientTasks);
            Console.WriteLine("All UDP client tasks completed.");
            return CommandRunnerResult.NoProcessing;
        }

        private async Task SendCommandsAsync(UdpClient udpClient, IPEndPoint remoteEndPoint, CancellationToken cToken)
        {
            try
            {
                string[] commands =
                [
                    "ts", "ts -v", "ts grp1", "ts grp1 cmd1", "ts grp1 grp2", "ts grp1 grp2 cmd2"
                ];

                Console.WriteLine("Sending commands individually...");
                foreach (string command in commands)
                {
                    await udpClient.SendSingleToTerminalAsync(command, TerminalIdentifiers.RemoteCommandDelimiter, TerminalIdentifiers.RemoteMessageDelimiter, Encoding.Unicode, remoteEndPoint, cToken);
                    Console.WriteLine($"Command sent: {command}");
                }

                // Send all commands as a batch
                Console.WriteLine("Sending all commands as a batch...");
                await udpClient.SendBatchToTerminalAsync(commands, TerminalIdentifiers.RemoteCommandDelimiter, TerminalIdentifiers.RemoteMessageDelimiter, Encoding.Unicode, remoteEndPoint, cToken);
                Console.WriteLine("Batch of commands sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during UDP send: {ex.Message}");
            }
            finally
            {
                udpClient.Dispose();
                Console.WriteLine("UDP connection closed.");
            }
        }

        private async Task StartClientAsync(string server, int port, CancellationToken cToken)
        {
            using var udpClient = new UdpClient();
            IPEndPoint remoteEndPoint = new(IPAddress.Parse(server), port);

            try
            {
                Console.WriteLine("UDP client ready to send data.");
                await SendCommandsAsync(udpClient, remoteEndPoint, cToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
        }

        private readonly IConfiguration configuration;
    }
}
