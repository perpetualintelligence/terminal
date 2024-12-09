using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestAuth.Runners
{
    /// <summary>
    /// Command runner for fetching user information from Microsoft Graph API after authentication.
    /// </summary>
    [CommandOwners("auth")]
    [CommandDescriptor("user", "Get user", "Fetches user information from Microsoft Graph API.", Commands.CommandType.Group, Commands.CommandFlags.None)]
    public class AuthUserRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="terminalConsole">Terminal console service.</param>
        /// <param name="httpClientFactory">HTTP client factory for making requests.</param>
        /// <param name="logger">Logger instance for logging.</param>
        public AuthUserRunner(ITerminalConsole terminalConsole, IHttpClientFactory httpClientFactory, ILogger<AuthUserRunner> logger)
        {
            _terminalConsole = terminalConsole;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Runs the command to fetch user information from Microsoft Graph API.
        /// </summary>
        /// <param name="context">Command runner context.</param>
        /// <returns>Command runner result.</returns>
        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            
            // Get the HTTP client from the factory with the name "demo-http" since this name is configured to use the
            // TestAuthDelegatingHandler in the Program.cs file.
            HttpClient httpClient = _httpClientFactory.CreateClient("demo-http");
            GraphServiceClient graphServiceClient = new(httpClient);

            try
            {
                // Get the current user's profile information. The OneImlx.Terminal framework will automatically trigger
                // the authentication flow if the user is not authenticated.
                await _terminalConsole.WriteLineAsync("Invoking Microsoft Graph API...");
                await _terminalConsole.WriteLineColorAsync(ConsoleColor.Magenta, "The test app will not store any data.");
                await Task.Delay(2000); // Simulate a delay
                Microsoft.Graph.Models.User? user = await graphServiceClient.Me.GetAsync();
                if (user == null)
                {
                    throw new System.Exception("User not found.");
                }

                await _terminalConsole.WriteLineAsync($"User information:");
                await _terminalConsole.WriteLineAsync($"Display Name: {user.DisplayName}");
                await _terminalConsole.WriteLineAsync($"User Principal Name: {user.UserPrincipalName}");
                await _terminalConsole.WriteLineAsync($"ID: {user.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user information from Microsoft Graph API.");
                throw;
            }

            return new CommandRunnerResult();
        }

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthUserRunner> _logger;
        private readonly ITerminalConsole _terminalConsole;
    }
}
