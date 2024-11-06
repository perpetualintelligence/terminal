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

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    [CommandOwners("send")]
    [CommandDescriptor("tcp", "Send TCP", "Send TCP commands to the server.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    public class SendTcpRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public SendTcpRunner(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            string server = configuration["testclient:testserver:ip"] ?? throw new InvalidOperationException("Server IP address is missing.");
            int port = configuration.GetValue<int>("testclient:testserver:port");

            var clientTasks = new Task[4];
            for (int i = 0; i < clientTasks.Length; i++)
            {
                clientTasks[i] = StartClientAsync(server, port, context.StartContext.TerminalCancellationToken);
            }

            await Task.WhenAll(clientTasks);
            Console.WriteLine("All client tasks completed successfully.");
            return CommandRunnerResult.NoProcessing;
        }

        private async Task SendCommandsAsync(TcpClient tcpClient, CancellationToken cToken)
        {
            try
            {
                string[] commands = ["ts", "ts -v", "ts grp1", "ts grp1 cmd1", "ts grp1 grp2", "ts grp1 grp2 cmd2"];

                Console.WriteLine("Sending commands individually...");
                foreach (string command in commands)
                {
                    await tcpClient.SendSingleAsync(command, TerminalIdentifiers.RemoteCommandDelimiter, TerminalIdentifiers.RemoteBatchDelimiter, Encoding.Unicode, cToken);
                    Console.WriteLine($"Command sent: {command}");
                }

                Console.WriteLine("Sending all commands as a batch...");
                await tcpClient.SendBatchAsync(commands, TerminalIdentifiers.RemoteCommandDelimiter, TerminalIdentifiers.RemoteBatchDelimiter, Encoding.Unicode, cToken);
                Console.WriteLine("Batch of commands sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while sending commands: {ex.Message}");
            }
            finally
            {
                tcpClient.Close();
                Console.WriteLine("Connection closed.");
            }
        }

        private async Task StartClientAsync(string server, int port, CancellationToken cToken)
        {
            try
            {
                using var tcpClient = new TcpClient();

                while (true)
                {
                    try
                    {
                        await tcpClient.ConnectAsync(IPAddress.Parse(server), port);
                        Console.WriteLine($"Connected to {tcpClient.Client.RemoteEndPoint}.");
                        break;
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Server not available, retrying in 5 seconds...");
                        await Task.Delay(5000, cToken);
                    }
                }

                await SendCommandsAsync(tcpClient, cToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
        }

        private readonly IConfiguration configuration;
    }
}
