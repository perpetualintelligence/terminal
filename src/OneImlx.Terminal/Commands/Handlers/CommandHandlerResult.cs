/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Runners;

namespace OneImlx.Terminal.Commands.Handlers
{
    /// <summary>
    /// The command handler result.
    /// </summary>
    public class CommandHandlerResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="checkerResult">The command checker result.</param>
        /// <param name="runnerResult">The command runner result.</param>
        public CommandHandlerResult(CommandCheckerResult checkerResult, CommandRunnerResult runnerResult)
        {
            RunnerResult = runnerResult;
            CheckerResult = checkerResult;
        }

        /// <summary>
        /// The command checker result.
        /// </summary>
        public CommandCheckerResult CheckerResult { get; }

        /// <summary>
        /// The command runner result.
        /// </summary>
        public CommandRunnerResult RunnerResult { get; }
    }
}
