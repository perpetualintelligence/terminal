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
    public abstract class CommandRunner<TResult> : IDelegateCommandRunner, ICommandRunner<TResult> where TResult : CommandRunnerResult
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<CommandRunnerResult> DelegateRunAsync(CommandRunnerContext context)
        {
            var result = await RunAsync(context);
            return (CommandRunnerResult)(object)result;
        }

        /// <inheritdoc/>
        public abstract Task<TResult> RunAsync(CommandRunnerContext context);
    }
}