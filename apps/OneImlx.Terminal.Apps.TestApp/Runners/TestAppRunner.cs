/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;

namespace OneImlx.Terminal.Apps.TestApp.Runners
{
    /// <summary>
    /// The root runner for the TestApp.
    /// </summary>
    [CommandDescriptor("test", "Test App", "Test application description.", Commands.CommandType.Root, Commands.CommandFlags.None)]
    [CommandRunner(typeof(TestAppRunner))]
    public class TestAppRunner : CommandRunner<CommandRunnerResult>, IDeclarativeTarget
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