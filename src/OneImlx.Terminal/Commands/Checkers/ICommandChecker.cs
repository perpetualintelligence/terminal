/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
{
    /// <summary>
    /// An abstraction to check a <see cref="Command"/>.
    /// </summary>
    public interface ICommandChecker
    {
        /// <summary>
        /// Checks <see cref="Command"/> asynchronously.
        /// </summary>
        /// <param name="context">The command check context.</param>
        /// <returns>The <see cref="CommandCheckerResult"/> instance.</returns>
        public Task<CommandCheckerResult> CheckCommandAsync(CommandCheckerContext context);
    }
}