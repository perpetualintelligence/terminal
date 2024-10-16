using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OneImlx.Terminal.Client.Extensions;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    /// <summary>
    /// Represents the runner for sending HTTP commands to the terminal server. This runner is part of the
    /// OneImlx.Terminal framework and demonstrates how to send commands via HTTP using the terminal framework.
    /// </summary>
    [CommandOwners("send")]
    [CommandDescriptor("http", "Send HTTP", "Send HTTP commands to the server.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    public class SendHttpRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendHttpRunner"/> class.
        /// </summary>
        /// <param name="configuration">The configuration object to retrieve server information.</param>
        public SendHttpRunner(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <summary>
        /// Executes the HTTP command runner. This method sends multiple HTTP POST requests to the terminal server.
        /// </summary>
        /// <param name="context">The context for the command runner execution.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            // Retrieve the server IP and port from the configuration.
            string ip = configuration.GetValue<string>("testclient:testserver:ip")
                        ?? throw new InvalidOperationException("The server IP address is missing.");
            string port = configuration.GetValue<string>("testclient:testserver:port")
                          ?? throw new InvalidOperationException("The server port is missing.");
            string serverTemplate = "http://{0}:{1}";
            string serverAddress = string.Format(serverTemplate, ip, port);

            // Create multiple tasks to send HTTP commands in parallel.
            Task[] clientTasks = new Task[4];
            for (int i = 0; i < clientTasks.Length; i++)
            {
                clientTasks[i] = StartHttpClientAsync(serverAddress, context.StartContext.TerminalCancellationToken);
            }

            // Wait for all client tasks to complete.
            await Task.WhenAll(clientTasks);
            Console.WriteLine("All HTTP client tasks completed successfully.");
            return CommandRunnerResult.NoProcessing;
        }

        /// <summary>
        /// Sends a batch of HTTP commands to the server. The commands are serialized and sent as a JSON request body.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> instance for making the HTTP request.</param>
        /// <param name="serverAddress">The address of the server to send commands to.</param>
        /// <param name="cancellationToken">The cancellation token to handle task cancellation.</param>
        private async Task SendHttpCommandsAsync(HttpClient client, string serverAddress, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("Sending individual HTTP commands...");

                // Array of commands to be sent to the server.
                string[] commands =
                [
                    "ts", "ts -v", "ts grp1", "ts grp1 cmd1", "ts grp1 grp2", "ts grp1 grp2 cmd2"
                ];

                // Iterate through each command and send it via HTTP POST.
                foreach (var command in commands)
                {
                    // Send individual command to the server.
                    var response = await client.SendSingleToTerminalAsync(command, TerminalIdentifiers.RemoteCommandDelimiter, TerminalIdentifiers.RemoteMessageDelimiter, cancellationToken);
                    response.EnsureSuccessStatusCode(); // Ensure the request was successful.
                    Console.WriteLine($"Sent command: {command}, Server Response: {await response.Content.ReadAsStringAsync()}");
                }

                // Send all commands in a single batch request.
                Console.WriteLine("Sending commands as a batch...");
                var batchResponse = await client.SendBatchToTerminalAsync(commands, TerminalIdentifiers.RemoteCommandDelimiter, TerminalIdentifiers.RemoteMessageDelimiter, cancellationToken);
                batchResponse.EnsureSuccessStatusCode();
                Console.WriteLine($"Batch sent. Server Response: {await batchResponse.Content.ReadAsStringAsync()}");
            }
            catch (HttpRequestException ex)
            {
                // Log and handle HTTP-specific exceptions.
                Console.WriteLine($"HTTP Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Catch and log general exceptions.
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Starts an HTTP client and sends multiple HTTP requests asynchronously.
        /// </summary>
        /// <param name="serverAddress">The address of the server to send requests to.</param>
        /// <param name="cancellationToken">The cancellation token to handle task cancellation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task StartHttpClientAsync(string serverAddress, CancellationToken cancellationToken)
        {
            try
            {
                HttpClient client = httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(serverAddress);
                await SendHttpCommandsAsync(client, serverAddress, cancellationToken); // Send the HTTP commands.
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur during the HTTP client operation.
                Console.WriteLine($"Error during HTTP client operation: {ex.Message}");
            }
        }

        // Private field to hold the configuration for retrieving server IP and port.
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;
    }
}
