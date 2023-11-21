/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Integration
{
    /// <summary>
    /// An abstraction to load assemblies within your terminal.
    /// </summary>
    public interface ITerminalCommandSourceAssemblyLoader<TContext>
    {
        /// <summary>
        /// Loads the assemblies asynchronously.
        /// </summary>
        /// <param name="context">The assembly load context.</param>
        public Task<IEnumerable<Assembly>> LoadAssembliesAsync(TContext context);
    }
}