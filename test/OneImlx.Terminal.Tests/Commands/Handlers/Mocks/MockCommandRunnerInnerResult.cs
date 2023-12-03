/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Runners;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Handlers.Mocks
{
    internal class MockCommandRunnerInnerResult : CommandRunnerResult
    {
        public static bool ResultDisposed { get; set; }

        public static bool ResultProcessed { get; set; }

        public override ValueTask DisposeAsync()
        {
            ResultDisposed = true;
            return new ValueTask();
        }

        public override Task ProcessAsync(CommandRunnerContext context, ILogger? logger = null)
        {
            ResultProcessed = true;
            return Task.CompletedTask;
        }
    }
}
