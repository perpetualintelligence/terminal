/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
{
    /// <summary>
    /// An abstraction to check an <see cref="Option"/>.
    /// </summary>
    public interface IOptionChecker
    {
        /// <summary>
        /// Checks <see cref="Option"/> asynchronously.
        /// </summary>
        /// <param name="context">The option check context.</param>
        /// <returns>The <see cref="OptionCheckerResult"/> instance.</returns>
        public Task<OptionCheckerResult> CheckAsync(OptionCheckerContext context);
    }
}