/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands.Runners
{
    /// <summary>
    /// The command runner result processor context.
    /// </summary>
    public class CommandRunnerResultProcessorContext
    {
        /// <summary>
        /// The command runner context.
        /// </summary>
        /// <param name="runnerContext"></param>
        public CommandRunnerResultProcessorContext(CommandRunnerContext runnerContext)
        {
            RunnerContext = runnerContext;
        }

        /// <summary>
        /// The command runner context.
        /// </summary>
        public CommandRunnerContext RunnerContext { get; set; }
    }
}
