/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.Runners
{
    [CommandOwners("send")]
    [CommandDescriptor("tcp", "Send TCP", "Send TCP commands to the server.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    public class SendTcpRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public SendTcpRunner(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            // Retrieves server IP and port from configuration, throws exception if not found
            string server = configuration.GetValue<string>("testclient:testserver:ip") ?? throw new InvalidOperationException("The server IP address is missing.");
            int port = configuration.GetValue<int>("testclient:testserver:port");

            // Initialize and start multiple client tasks
            Task[] clientTasks = new Task[4];
            for (int i = 0; i < clientTasks.Length; i++)
            {
                clientTasks[i] = StartClient(server, port, context.StartContext.TerminalCancellationToken);
            }

            // Wait for all client tasks to complete
            await Task.WhenAll(clientTasks);
            Console.WriteLine("Test client finished.");

            return CommandRunnerResult.NoProcessing;
        }

        // Method to send a series of commands to the server
        private async Task SendServerCommandsAsync(Socket socket, CancellationToken cToken)
        {
            try
            {
                Console.WriteLine($"Sending delimited individual commands from {socket.LocalEndPoint}");

                // List of commands to send
                string[] individualCommands = new string[]
                {
                    "ts",
                    "ts -v",
                    "ts grp1",
                    "ts grp1 cmd1",
                    "ts grp1 grp2",
                    "ts grp1 grp2 cmd2",
                };

                // Loop through the commands, sending each one after formatting
                foreach (string message in individualCommands)
                {
                    string delimitedMessage = TerminalServices.DelimitedMessage(TerminalIdentifiers.RemoteCommandDelimiter, TerminalIdentifiers.RemoteMessageDelimiter, message);
                    byte[] data = Encoding.Unicode.GetBytes(delimitedMessage);
                    await socket.SendAsync(data, SocketFlags.None, cToken);
                    Console.WriteLine($"Sent {delimitedMessage} from {socket.LocalEndPoint}");
                }

                // Optionally, send all commands at once as a single concatenated message
                Console.WriteLine("Sending delimited multiple commands...");
                string concatenatedCommands = TerminalServices.DelimitedMessage(TerminalIdentifiers.RemoteCommandDelimiter, TerminalIdentifiers.RemoteMessageDelimiter, individualCommands);
                byte[] allData = Encoding.Unicode.GetBytes(concatenatedCommands);
                await socket.SendAsync(allData, SocketFlags.None, cToken);
                Console.WriteLine($"Sent {concatenatedCommands} from {socket.LocalEndPoint}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }
            finally
            {
                // Ensure the socket is properly closed on completion
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                Console.WriteLine("Connection closed.");
            }
        }

        // Method to initialize and start a TCP client
        private async Task StartClient(string server, int port, CancellationToken cToken)
        {
            try
            {
                // Create a new TCP socket
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Attempt to connect to the specified server and port
                while (true)
                {
                    try
                    {
                        Console.WriteLine("Attempting to connect to server...");
                        await client.ConnectAsync(IPAddress.Parse(server), port);
                        Console.WriteLine($"Connected to {client.RemoteEndPoint} from {client.LocalEndPoint}.");
                        break;
                    }
                    catch (SocketException)
                    {
                        // Handle cases where the server is not available for connection
                        Console.WriteLine("Server not available, retrying in 5 seconds...");
                        await Task.Delay(5000, cToken);
                    }
                }

                // Once connected, send commands to the server
                await SendServerCommandsAsync(client, cToken);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during client operation: {e}");
            }
        }

        private readonly IConfiguration configuration;
    }
}
