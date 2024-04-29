/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Net;
using System.Net.Sockets;
using System.Text;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.TestClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            cts = new CancellationTokenSource();
            Console.WriteLine("Starting test client...");

            // Define the server address and port
            string server = "127.0.0.1";
            int port = 49153; // Example port

            // Start all clients simultaneously
            Task[] clientTasks =
            {
                StartClient(server, port),
                StartClient(server, port),
                StartClient(server, port),
                StartClient(server, port)
            };

            // Wait for all to complete
            await Task.WhenAll(clientTasks);

            Console.WriteLine("Test client finished.");
        }

        private static async Task SendIndefinetilyAsync(Socket socket)
        {
            try
            {
                while (true)
                {
                    cts.Token.ThrowIfCancellationRequested();

                    Console.WriteLine("Sending delimited individual commands...");

                    // Prepare the message to send
                    string[] individualCommands = [
                        "test",
                        "test -v",
                        "test grp1",
                        "test grp1 cmd1",
                        "test grp1 invalid",
                        "test grp1 grp2",
                        "test grp1 grp2 cmd2",
                        "test grp1 grp2 invalid"
                    ];

                    // Send each message over tcp with a delay of 500 ms
                    // NOTE: The TestServer enables the remote delimited message feature and uses default $m$ as a delimiter
                    // so we need to delimit each message.
                    byte[] data;
                    foreach (string message in individualCommands)
                    {
                        string delimitedMessage = TerminalServices.DelimitedMessage(TerminalIdentifiers.RemoteCommandDelimiter, TerminalIdentifiers.RemoteMessageDelimiter, message);
                        data = Encoding.Unicode.GetBytes(delimitedMessage);
                        await socket.SendAsync(data, SocketFlags.None, cts.Token);
                        Console.WriteLine("Sent: {0}", delimitedMessage);
                    }

                    // Wait a bit before sending the next message lot
                    Console.WriteLine("Waiting for 5 seconds...");
                    Thread.Sleep(5000);

                    // Send delimited message
                    Console.WriteLine("Sending delimited multiple commands...");
                    string concatenatedCommands = TerminalServices.DelimitedMessage(TerminalIdentifiers.RemoteCommandDelimiter, TerminalIdentifiers.RemoteMessageDelimiter, individualCommands);
                    data = Encoding.Unicode.GetBytes(concatenatedCommands);
                    await socket.SendAsync(data, SocketFlags.None, cts.Token);
                    Console.WriteLine("Sent: {0}", concatenatedCommands);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
            finally
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                Console.WriteLine("Connection closed.");
            }
        }

        private static async Task StartClient(string server, int port)
        {
            try
            {
                // Create a TCP/IP socket
                Socket client = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the server
                while (true)
                {
                    try
                    {
                        Console.WriteLine("Attempting to connect to server...");
                        await client.ConnectAsync(IPAddress.Parse(server), port);
                        Console.WriteLine("Connected to {0} from {1}.", client.RemoteEndPoint, client.LocalEndPoint);
                        break; // Exit the loop once connected
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Server not available, retrying in 5 seconds...");
                        Thread.Sleep(5000);
                    }
                }

                await SendIndefinetilyAsync(client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static CancellationTokenSource cts;
    }
}
