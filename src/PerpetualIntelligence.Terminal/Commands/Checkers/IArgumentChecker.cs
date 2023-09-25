/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
{
    /// <summary>
    /// An abstraction to check an <see cref="Argument"/>.
    /// </summary>
    public interface IArgumentChecker
    {
        /// <summary>
        /// Checks <see cref="Argument"/> asynchronously.
        /// </summary>
        /// <param name="context">The argument check context.</param>
        /// <returns>The <see cref="ArgumentCheckerResult"/> instance.</returns>
        public Task<ArgumentCheckerResult> CheckAsync(ArgumentCheckerContext context);
    }
}