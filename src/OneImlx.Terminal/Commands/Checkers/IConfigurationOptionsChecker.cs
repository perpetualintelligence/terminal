/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Configuration.Options;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Checkers
{
    /// <summary>
    /// An abstraction of <see cref="TerminalOptions"/> checker.
    /// </summary>
    public interface IConfigurationOptionsChecker
    {
        /// <summary>
        /// Checks the terminal configuration options asynchronously.
        /// </summary>
        /// <param name="context">The configuration options check context.</param>
        public Task CheckAsync(TerminalOptions context);
    }
}