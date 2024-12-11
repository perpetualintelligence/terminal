/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Runners;

namespace OneImlx.Terminal.Commands.Routers
{
    /// <summary>
    /// Represents the result of a command router.
    /// </summary>
    public sealed class CommandRouterResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRouterResult"/> class.
        /// </summary>
        public CommandRouterResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRouterResult"/> class with specified checker and runner results.
        /// </summary>
        /// <param name="checkerResult">The result of the command checker.</param>
        /// <param name="runnerResult">The result of the command runner.</param>
        public CommandRouterResult(CommandCheckerResult checkerResult, CommandRunnerResult runnerResult)
        {
            CheckerResult = checkerResult;
            RunnerResult = runnerResult;
        }

        /// <summary>
        /// Gets the result of the command checker.
        /// </summary>
        public CommandCheckerResult? CheckerResult { get; internal set; }

        /// <summary>
        /// Gets the result of the command runner.
        /// </summary>
        public CommandRunnerResult? RunnerResult { get; internal set; }

        /// <summary>
        /// Ensures that the CheckerResult is not null.
        /// </summary>
        /// <exception cref="TerminalException">Thrown if CheckerResult is null.</exception>
        public CommandCheckerResult EnsureCheckerResult()
        {
            if (CheckerResult == null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The command checker result is not set.");
            }

            return CheckerResult;
        }

        /// <summary>
        /// Ensures that the RunnerResult is not null.
        /// </summary>
        /// <exception cref="TerminalException">Thrown if RunnerResult is null.</exception>
        public CommandRunnerResult EnsureRunnerResult()
        {
            if (RunnerResult == null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The command runner result is not set.");
            }

            return RunnerResult;
        }
    }
}
