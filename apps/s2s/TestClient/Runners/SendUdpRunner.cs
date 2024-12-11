using Microsoft.Extensions.Configuration;
using OneImlx.Terminal.Client.Extensions;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    [CommandOwners("send")]
    [CommandDescriptor("udp", "Send UDP", "Send UDP commands to the server.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    public class SendUdpRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public SendUdpRunner(ITerminalTextHandler terminalTextHandler, ITerminalConsole terminalConsole, IConfiguration configuration)
        {
            this.terminalTextHandler = terminalTextHandler;
            this.terminalConsole = terminalConsole;
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRouterContext context)
        {
            string server = configuration.GetValue<string>("testclient:testserver:ip")
                            ?? throw new InvalidOperationException("Server IP address is missing.");
            int port = configuration.GetValue<int?>("testclient:testserver:port")
                           ?? throw new InvalidOperationException("Server port is missing or invalid.");

            await terminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "UDP concurrent and asynchronous demo");

            var clientTasks = new Task[5];
            for (int idx = 0; idx < clientTasks.Length; idx++)
            {
                clientTasks[idx] = StartClientAsync(server, port, idx, context.TerminalContext.StartContext.TerminalCancellationToken);
            }

            await Task.WhenAll(clientTasks);
            await terminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "UDP client tasks completed successfully.");
            return new CommandRunnerResult();
        }

        private async Task SendCommandsAsync(UdpClient udpClient, IPEndPoint remoteEndPoint, int clientIndex, CancellationToken cToken)
        {
            string[] cmdIds = ["cmd1", "cmd2", "cmd3", "cmd4", "cmd5", "cmd6"];
            string[] commands = ["ts", "ts -v", "ts grp1", "ts grp1 cmd1", "ts grp1 grp2", "ts grp1 grp2 cmd2"];

            try
            {
                foreach (var (cmdId, command) in cmdIds.Zip(commands))
                {
                    TerminalInput single = TerminalInput.Single(cmdId, command);
                    await udpClient.SendToTerminalAsync(single, TerminalIdentifiers.StreamDelimiter, remoteEndPoint, cToken);
                    await terminalConsole.WriteLineAsync($"[Client {clientIndex}] Request=\"{cmdId}\" Raw=\"{command}\" => Sent");
                }

                string batchId = $"batch{clientIndex}";
                TerminalInput batch = TerminalInput.Batch(batchId, cmdIds, commands);
                await udpClient.SendToTerminalAsync(batch, TerminalIdentifiers.StreamDelimiter, remoteEndPoint, cToken);
                await terminalConsole.WriteLineAsync($"[Client {clientIndex}] BatchId=\"{batchId}\" => Batch Sent");
            }
            catch (Exception ex)
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] Error: {ex.Message}");
            }
        }

        private async Task StartClientAsync(string server, int port, int clientIndex, CancellationToken cToken)
        {
            using var udpClient = new UdpClient();
            IPEndPoint remoteEndPoint = new(IPAddress.Parse(server), port);

            try
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, $"UDP client {clientIndex} initialized for {server}:{port}...");
                await SendCommandsAsync(udpClient, remoteEndPoint, clientIndex, cToken);
            }
            catch (Exception ex)
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, $"[Client {clientIndex}] Error: {ex.Message}");
            }
            finally
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, $"[Client {clientIndex}] Connection closed.");
            }
        }

        private readonly IConfiguration configuration;
        private readonly ITerminalConsole terminalConsole;
        private readonly ITerminalTextHandler terminalTextHandler;
    }
}
