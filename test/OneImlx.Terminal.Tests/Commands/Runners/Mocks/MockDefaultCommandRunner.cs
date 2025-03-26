﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Commands.Handlers.Mocks;

namespace OneImlx.Terminal.Commands.Runners.Mocks
{
    internal class MockDefaultCommandRunner : CommandRunner<CommandRunnerResult>
    {
        public bool HelpCalled { get; private set; }

        public bool RunCalled { get; private set; }

        public override Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            RunCalled = true;
            return Task.FromResult((CommandRunnerResult)new MockCommandRunnerInnerResult());
        }

        public override async Task RunHelpAsync(CommandContext context)
        {
            HelpCalled = true;
            await base.RunHelpAsync(context);
        }
    }
}
