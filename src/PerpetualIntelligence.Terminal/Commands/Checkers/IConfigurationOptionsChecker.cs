/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Configuration.Options;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
{
    /// <summary>
    /// An abstraction of <see cref="TerminalOptions"/> checker.
    /// </summary>
    public interface IConfigurationOptionsChecker
    {
        /// <summary>
        /// Checks the <c>pi-cli</c> terminal configuration options asynchronously.
        /// </summary>
        /// <param name="context">The configuration options check context.</param>
        public Task CheckAsync(TerminalOptions context);
    }
}