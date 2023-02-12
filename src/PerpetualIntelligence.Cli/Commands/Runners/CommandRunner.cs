/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Runners
{
    /// <summary>
    /// The command runner.
    /// </summary>
    public abstract class CommandRunner<TContext, TResult> : ICommandRunner<TContext, TResult> where TContext : CommandRunnerContext where TResult : CommandRunnerResult
    {
        /// <inheritdoc/>
        public abstract Task<TResult> RunAsync(TContext context);
    }
}