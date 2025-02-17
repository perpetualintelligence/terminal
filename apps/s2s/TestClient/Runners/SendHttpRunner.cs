using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OneImlx.Shared.Infrastructure;
using OneImlx.Terminal.Client.Extensions;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    [CommandOwners("send")]
    [CommandDescriptor("http", "HTTP test", "Send HTTP commands to the terminal server.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    public class SendHttpRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public SendHttpRunner(IConfiguration configuration, ITerminalConsole terminalConsole, IHttpClientFactory httpClientFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.terminalConsole = terminalConsole ?? throw new ArgumentNullException(nameof(terminalConsole));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
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

                await terminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "HTTP concurrent and asynchronous demo");

                var clientTasks = new Task[maxClients];
                for (int idx = 0; idx < clientTasks.Length; idx++)
                {
                    clientTasks[idx] = StartHttpClientAsync(serverAddress, idx, context.TerminalContext.TerminalCancellationToken);
                }

                await Task.WhenAll(clientTasks);
                return new CommandRunnerResult();
            }
            finally
            {
                stopwatch.Stop();
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Green, $"{maxClients * 14} requests completed by {maxClients} HTTP client tasks in {stopwatch.Elapsed.TotalMilliseconds} milliseconds.");
            }
        }

        private async Task SendHttpCommandsAsync(HttpClient client, int clientIndex, CancellationToken cToken)
        {
            string[] cmdIds = ["cmd1", "cmd2", "cmd3", "cmd4", "cmd5", "cmd6", "cmd7"];
            string[] commands = ["ts", "ts -v", "ts grp1", "ts grp1 cmd1", "ts grp1 grp2", "ts grp1 grp2 cmd2", "ts grp1 grp2 cmd2 invalid"];

            try
            {
                foreach (var (cmdId, command) in cmdIds.Zip(commands))
                {
                    TerminalInput single = TerminalInput.Single(cmdId, command);
                    TerminalOutput? output = await client.SendToTerminalAsync(single, cToken);

                    if (output == null)
                    {
                        await terminalConsole.WriteLineAsync($"[Client {clientIndex}] Request=\"{cmdId}\" Raw=\"{command}\" => No Response");
                        continue;
                    }

                    string result = output.Input.Requests[0].Result?.ToString() ?? "No Result";
                    if (output.Input.Requests[0].IsError)
                    {
                        Error error = output.GetDeserializedResult<Error>(0);
                        result = error.FormatDescription();

                        await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] Request=\"{cmdId}\" Raw=\"{command}\" => Result={result}");
                    }
                    else
                    {
                        await terminalConsole.WriteLineAsync($"[Client {clientIndex}] Request=\"{cmdId}\" Raw=\"{command}\" => Result={result}");
                    }
                }

                string batchId = $"batch{clientIndex}";
                TerminalInput batch = TerminalInput.Batch(batchId, cmdIds, commands);
                TerminalOutput? batchOutput = await client.SendToTerminalAsync(batch, cToken);
                for (int idx = 0; idx < batch.Requests.Length; ++idx)
                {
                    var request = batch.Requests[idx];
                    if (batchOutput == null)
                    {
                        await terminalConsole.WriteLineAsync($"[Client {clientIndex}] BatchId=\"{batchId}\" Request=\"{request.Id}\" Raw=\"{request.Raw}\" => Result={"No Response"}");
                        continue;
                    }

                    string result = batchOutput.Input.Requests[idx].Result?.ToString() ?? "No Result";
                    if (batchOutput.Input.Requests[idx].IsError)
                    {
                        Error error = batchOutput.GetDeserializedResult<Error>(idx);
                        result = error.FormatDescription();
                        await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] BatchId=\"{batchId}\" Request=\"{request.Id}\" Raw=\"{request.Raw}\" => Result={result ?? "No Response"}");
                    }
                    else
                    {
                        await terminalConsole.WriteLineAsync($"[Client {clientIndex}] BatchId=\"{batchId}\" Request=\"{request.Id}\" Raw=\"{request.Raw}\" => Result={result ?? "No Response"}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] HTTP Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] Error: {ex.Message}");
            }
        }

        private async Task StartHttpClientAsync(string serverAddress, int clientIndex, CancellationToken cToken)
        {
            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(serverAddress);

            try
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, $"HTTP client {clientIndex} initialized for {serverAddress}...");
                await SendHttpCommandsAsync(client, clientIndex, cToken);
            }
            catch (Exception ex)
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] {ex.Message}");
            }
            finally
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, $"HTTP client {clientIndex} disposed.");
            }
        }

        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly Stopwatch stopwatch = new();
        private readonly ITerminalConsole terminalConsole;
    }
}
