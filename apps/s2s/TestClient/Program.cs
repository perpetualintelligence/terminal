/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OneImlx.Terminal.Apps.TestClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Starting test client...");

            // Define the server address and port
            string server = "127.0.0.1";
            int port = 49153; // Example port

            // Create a new TCP client
            TcpClient client = new();

            // Try to connect to the server repeatedly until successful
            while (true)
            {
                try
                {
                    Console.WriteLine("Attempting to connect to server...");
                    client.Connect(IPAddress.Parse(server), port);
                    Console.WriteLine("Connected to the server on {0}.", server);
                    break; // Exit the loop once connected
                }
                catch (SocketException)
                {
                    Console.WriteLine("Server not available, retrying in 5 seconds...");
                    Thread.Sleep(5000); // Wait for 5 seconds before retrying
                }
            }

            try
            {
                // Get the stream for writing and reading through the socket
                NetworkStream stream = client.GetStream();

                while (true)
                {
                    // Prepare the message to send
                    string[] messages = [
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
                    foreach (string message in messages)
                    {
                        byte[] data = Encoding.Unicode.GetBytes(message);
                        stream.Write(data, 0, data.Length);
                        Console.WriteLine("Sent: {0}", message);
                        Thread.Sleep(500);
                    }

                    // Wait a bit before sending the next message lot
                    Thread.Sleep(5000);

                    throw new Exception("Stop");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
            finally
            {
                // Cleanup
                if (client != null && client.Connected)
                {
                    client.Close();
                }
                Console.WriteLine("Connection closed.");
            }
        }
    }
}