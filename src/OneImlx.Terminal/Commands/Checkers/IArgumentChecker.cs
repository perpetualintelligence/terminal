/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Checkers
{
    /// <summary>
    /// An abstraction to check an <see cref="Argument"/>.
    /// </summary>
    public interface IArgumentChecker
    {
        /// <summary>
        /// Checks <see cref="Argument"/> asynchronously.
        /// </summary>
        /// <param name="argument">The argument to check.</param>
        /// <returns>The <see cref="ArgumentCheckerResult"/> instance.</returns>
        public Task<ArgumentCheckerResult> CheckArgumentAsync(Argument argument);
    }
}
