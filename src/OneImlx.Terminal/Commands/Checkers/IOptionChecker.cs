/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Checkers
{
    /// <summary>
    /// An abstraction to check an <see cref="Option"/>.
    /// </summary>
    public interface IOptionChecker
    {
        /// <summary>
        /// Checks <see cref="Option"/> asynchronously.
        /// </summary>
        /// <param name="option">The option to context.</param>
        /// <returns>The <see cref="OptionCheckerResult"/> instance.</returns>
        public Task<OptionCheckerResult> CheckOptionAsync(Option option);
    }
}
