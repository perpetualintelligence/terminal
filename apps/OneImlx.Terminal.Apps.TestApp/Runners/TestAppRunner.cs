/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;

namespace OneImlx.Terminals.Net8.Runners
{
    /// <summary>
    /// The root runner for the TestApp.
    /// </summary>
    [CommandDescriptor("test", "Test App", "Test application description.", Terminal.Commands.CommandType.Root, Terminal.Commands.CommandFlags.None)]
    public class TestAppRunner : CommandRunner<CommandRunnerResult>
    {
        public TestAppRunner(ILogger<TestAppRunner> logger)
        {
            
        }
        public override Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            throw new NotImplementedException();
        }
    }
}