/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Runners
{
    /// <summary>
    /// The clear screen command runner.
    /// </summary>
    public class ClearScreenRunner : CommandRunner<CommandRunnerResult>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ClearScreenRunner()
        {
        }

        /// <inheritdoc/>
        public override Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            Console.Clear();
            return Task.FromResult(new CommandRunnerResult());
        }
    }
}