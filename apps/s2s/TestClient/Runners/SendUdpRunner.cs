using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.Runners
{
    [CommandOwners("send")]
    [CommandDescriptor("udp", "Send UDP", "Send UDP commands to the server.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    public class SendUdpRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly IConfiguration configuration;

        public SendUdpRunner(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            // Extracts server details from configuration or throws if missing
            string server = configuration.GetValue<string>("testclient:testserver:ip") ?? throw new InvalidOperationException("The server IP address is missing.");
            int port = configuration.GetValue<int>("testclient:testserver:port");

            // Creates and starts multiple UDP client tasks
            Task[] clientTasks =
            {
                StartClient(server, port, Guid.NewGuid().ToString(), context.StartContext.TerminalCancellationToken),
                StartClient(server, port, Guid.NewGuid().ToString(), context.StartContext.TerminalCancellationToken),
                StartClient(server, port, Guid.NewGuid().ToString(), context.StartContext.TerminalCancellationToken),
                StartClient(server, port, Guid.NewGuid().ToString(), context.StartContext.TerminalCancellationToken)
            };

            // Waits for all client tasks to complete
            await Task.WhenAll(clientTasks);
            Console.WriteLine("UDP test client finished.");

            return CommandRunnerResult.NoProcessing;
        }

        // Sends commands to the server using UDP
        private async Task SendServerCommandsAsync(UdpClient udpClient, IPEndPoint remoteEndPoint, CancellationToken cToken)
        {
            try
            {
                // Commands to send
                string[] individualCommands =
                {
                    "ts",
                    "ts -v",
                    "ts grp1",
                    "ts grp1 cmd1",
                    "ts grp1 grp2",
                    "ts grp1 grp2 cmd2",
                };

                // Send each command after formatting it for UDP transport
                foreach (string message in individualCommands)
                {
                    string delimitedMessage = TerminalServices.DelimitedMessage(TerminalIdentifiers.RemoteCommandDelimiter, TerminalIdentifiers.RemoteMessageDelimiter, message);
                    byte[] data = Encoding.Unicode.GetBytes(delimitedMessage);
                    await udpClient.SendAsync(data, data.Length, remoteEndPoint);
                    Console.WriteLine("Sent {0} to {1}", delimitedMessage, remoteEndPoint);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception during UDP send: {0}", e.Message);
            }
            finally
            {
                // Close the UDP client after sending commands
                udpClient.Close();
                Console.WriteLine("UDP connection closed.");
            }
        }

        // Initializes and starts a UDP client
        private async Task StartClient(string server, int port, string clientId, CancellationToken cToken)
        {
            using (UdpClient udpClient = new UdpClient())
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(server), port);

                try
                {
                    Console.WriteLine($"UDP client {clientId} ready to send data.");
                    await SendServerCommandsAsync(udpClient, remoteEndPoint, cToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
