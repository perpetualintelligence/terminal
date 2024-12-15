using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OneImlx.Terminal.Client.Extensions;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    [CommandOwners("send")]
    [CommandDescriptor("http", "Send HTTP", "Send HTTP commands to the server.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
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
            try
            {
                stopwatch.Restart();
                string ip = configuration["testclient:testserver:ip"] ?? throw new InvalidOperationException("Server IP address is missing.");
                string port = configuration.GetValue<string>("testclient:testserver:port") ?? throw new InvalidOperationException("Server port is missing.");
                string serverAddress = $"http://{ip}:{port}";

                await terminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "HTTP concurrent and asynchronous demo");

                var clientTasks = new Task[5];
                for (int idx = 0; idx < clientTasks.Length; idx++)
                {
                    clientTasks[idx] = StartHttpClientAsync(serverAddress, idx, context.TerminalContext.StartContext.TerminalCancellationToken);
                }

                await Task.WhenAll(clientTasks);
                return new CommandRunnerResult();
            }
            finally
            {
                stopwatch.Stop();
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Green, $"HTTP client tasks completed in {stopwatch.Elapsed.TotalMilliseconds} milliseconds.");
            }
        }

        private async Task SendHttpCommandsAsync(HttpClient client, int clientIndex, CancellationToken cToken)
        {
            string[] cmdIds = ["cmd1", "cmd2", "cmd3", "cmd4", "cmd5", "cmd6"];
            string[] commands = ["ts", "ts -v", "ts grp1", "ts grp1 cmd1", "ts grp1 grp2", "ts grp1 grp2 cmd2"];

            try
            {
                foreach (var (cmdId, command) in cmdIds.Zip(commands))
                {
                    TerminalInput single = TerminalInput.Single(cmdId, command);
                    var output = await client.SendToTerminalAsync(single, cToken);

                    await terminalConsole.WriteLineAsync($"[Client {clientIndex}] Request=\"{cmdId}\" Raw=\"{command}\" => Result={output?.Results[0] ?? "No Response"}");
                }

                string batchId = $"batch{clientIndex}";
                TerminalInput batch = TerminalInput.Batch(batchId, cmdIds, commands);
                var batchOutput = await client.SendToTerminalAsync(batch, cToken);
                for (int idx = 0; idx < batchOutput!.Input.Requests.Length; ++idx)
                {
                    var request = batchOutput.Input.Requests[idx];
                    var result = batchOutput.Results[idx];
                    await terminalConsole.WriteLineAsync($"[Client {clientIndex}] BatchId=\"{batchId}\" Request=\"{request.Id}\" Raw=\"{request.Raw}\" => Result={result ?? "No Response"}");
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
