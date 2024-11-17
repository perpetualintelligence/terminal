using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OneImlx.Terminal.Client.Extensions;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    [CommandOwners("send")]
    [CommandDescriptor("tcp", "Send TCP", "Send TCP commands to the server.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    public class SendTcpRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public SendTcpRunner(ITerminalTextHandler terminalTextHandler, ITerminalConsole terminalConsole, IConfiguration configuration)
        {
            this.terminalTextHandler = terminalTextHandler;
            this.terminalConsole = terminalConsole;
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            string server = configuration["testclient:testserver:ip"] ?? throw new InvalidOperationException("Server IP address is missing.");
            int port = configuration.GetValue<int>("testclient:testserver:port");

            var clientTasks = new Task[5];
            for (int i = 0; i < clientTasks.Length; i++)
            {
                clientTasks[i] = StartClientAsync(server, port, context.StartContext.TerminalCancellationToken);
            }

            await Task.WhenAll(clientTasks);
            await terminalConsole.WriteLineAsync("All client tasks completed successfully.");
            return new CommandRunnerResult();
        }

        private async Task SendCommandsAsync(TcpClient tcpClient, CancellationToken cToken)
        {
            string[] commands = ["ts", "ts -v", "ts grp1", "ts grp1 cmd1", "ts grp1 grp2", "ts grp1 grp2 cmd2"];

            await terminalConsole.WriteLineAsync("Sending commands individually...");
            foreach (string command in commands)
            {
                await tcpClient.SendSingleAsync(command, TerminalIdentifiers.RemoteCommandDelimiter, TerminalIdentifiers.RemoteBatchDelimiter, terminalTextHandler.Encoding, cToken);
                await terminalConsole.WriteLineAsync($"Request: {command}");
            }

            await terminalConsole.WriteLineAsync("Sending all commands as a batch...");
            await tcpClient.SendBatchAsync(commands, TerminalIdentifiers.RemoteCommandDelimiter, TerminalIdentifiers.RemoteBatchDelimiter, Encoding.Unicode, cToken);

            await Task.Delay(1000);

            // Read responses from the server until no more data is available
            await terminalConsole.WriteLineAsync("Reading responses...");
            NetworkStream stream = tcpClient.GetStream();
            try
            {
                byte[] buffer = new byte[2048];
                if (stream.DataAvailable)
                {
                    int bytesRead = await stream.ReadAsync(buffer, cToken);
                    if (bytesRead > 0)
                    {
                        string? response = JsonSerializer.Deserialize<string>(buffer.Take(bytesRead).ToArray());
                        await terminalConsole.WriteLineAsync($"Response received: {response}");
                    }
                }
            }
            finally
            {
                tcpClient.Close();
                await terminalConsole.WriteLineAsync("Connection closed.");
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
                        await terminalConsole.WriteLineAsync($"Connected to {tcpClient.Client.RemoteEndPoint}.");
                        break;
                    }
                    catch (SocketException)
                    {
                        await terminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "Server not available, retrying in 5 seconds...");
                        await Task.Delay(5000, cToken);
                    }
                }

                await SendCommandsAsync(tcpClient, cToken);
            }
            catch (Exception ex)
            {
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, "Client error. info={0}", ex.Message);
            }
        }

        private readonly IConfiguration configuration;
        private readonly ITerminalConsole terminalConsole;
        private readonly ITerminalTextHandler terminalTextHandler;
    }
}
