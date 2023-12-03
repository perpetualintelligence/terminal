/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Integration
{
    /// <summary>
    /// An abstraction to checks the <see cref="ITerminalCommandSource{TContext}"/>.
    /// </summary>
    public interface ITerminalCommandSourceChecker<TContext> where TContext : class
    {
        /// <summary>
        /// Checks the source and throws an exception if the source is not valid.
        /// </summary>
        /// <returns></returns>
        public Task CheckSourceAsync(TContext context);
    }
}