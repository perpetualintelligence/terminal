/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Terminal.Commands.Runners;

namespace PerpetualIntelligence.Terminal.Commands.Handlers
{
    /// <summary>
    /// The command handler result.
    /// </summary>
    public class CommandHandlerResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="runnerResult">The command runner result.</param>
        /// <param name="checkerResult">The command checker result.</param>
        public CommandHandlerResult(CommandRunnerResult runnerResult, CommandCheckerResult checkerResult)
        {
            RunnerResult = runnerResult;
            CheckerResult = checkerResult;
        }

        /// <summary>
        /// The command runner result.
        /// </summary>
        public CommandRunnerResult RunnerResult { get; }

        /// <summary>
        /// The command checker result.
        /// </summary>
        public CommandCheckerResult CheckerResult { get; }
    }
}