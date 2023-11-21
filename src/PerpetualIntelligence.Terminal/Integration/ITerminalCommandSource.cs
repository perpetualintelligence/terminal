/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Integration
{
    /// <summary>
    /// An abstraction of a terminal command source.
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    public interface ITerminalCommandSource<TContext> where TContext : class
    {
        /// <summary>
        /// Loads the terminal command source asynchronously.
        /// </summary>
        /// <param name="context">The terminal source context.</param>
        /// <returns>
        /// This interface allows for different sources of terminal commands, either from local or remote origins.
        /// It supports customization via the <typeparamref name="TContext"/> parameter, which can be tailored
        /// to specific loading contexts (e.g., configurations, authentication). Implementers can define how commands
        /// are loaded, catering to various application needs.
        /// </returns>
        public Task LoadCommandSourceAsync(TContext context);
    }
}