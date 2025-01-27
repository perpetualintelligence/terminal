/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Dynamics
{
    /// <summary>
    /// The default <see cref="AssemblyLoadContext"/> based <see cref="ITerminalCommandSourceAssemblyLoader{TContext}"/>.
    /// </summary>
    /// <remarks>
    /// This class provides a basic implementation for loading command source assemblies from a given context.
    /// It loads each assembly specified in the <see cref="PublishedCommandSourceContext"/> without resolving conflicts
    /// that might arise from different versions of the same assembly or dependencies. The loader checks if an assembly
    /// is already loaded before attempting to load it again and logs a warning if so.
    ///
    /// The default implementation is straightforward but does not handle advanced scenarios such as version conflicts
    /// or loading assemblies with different dependency trees. For more complex requirements, including advanced conflict
    /// resolution and dependency management, developers should create a custom loader by implementing the
    /// <see cref="ITerminalCommandSourceAssemblyLoader{TContext}"/> interface with their own logic.
    /// </remarks>
    public sealed class PublishedAssemblyLoader : ITerminalCommandSourceAssemblyLoader<PublishedCommandSourceContext>
    {
        private readonly ILogger<PublishedAssemblyLoader> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedAssemblyLoader"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public PublishedAssemblyLoader(ILogger<PublishedAssemblyLoader> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Asynchronously loads the primary command source assemblies specified in the <paramref name="context"/>.
        /// While loading these primary assemblies, dependency assemblies may also be loaded. However, the returned
        /// enumerable only includes the primary assemblies that are requested by the context, not the dependencies.
        /// </summary>
        /// <param name="context">The context containing information about the primary command source assemblies to load,
        /// such as their paths.</param>
        /// <returns>A task representing the asynchronous operation, with a result of an enumerable of the loaded
        /// primary command source assemblies.</returns>
        public Task<IEnumerable<Assembly>> LoadAssembliesAsync(PublishedCommandSourceContext context)
        {
            List<Assembly> assemblies = [];

            foreach (var kvp in context.PublishedAssemblies)
            {
                string assemblyPath = Path.Combine(kvp.Value, kvp.Key);
                if (!File.Exists(assemblyPath))
                {
                    throw new TerminalException(TerminalErrors.ServerError, "The published command source assembly does not exist. path={0}", assemblyPath);
                }

                var assemblyName = AssemblyName.GetAssemblyName(assemblyPath);

                // Check if the assembly is already loaded
                var currentLoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                var existingAssembly = currentLoadedAssemblies.FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(a.GetName(), assemblyName));
                if (existingAssembly != null)
                {
                    assemblies.Add(existingAssembly);
                    logger.LogWarning("Assembly already loaded, load path ignored. path={0} assembly={1}", assemblyPath, assemblyName);
                    continue;
                }

                // Load the assembly
                Assembly loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                logger.LogInformation("Loaded assembly. assembly={0}", assemblyName);

                // Log dependent assemblies
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    var newLoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                    var diffAssemblies = newLoadedAssemblies.Except(currentLoadedAssemblies);

                    foreach (Assembly asm in diffAssemblies)
                    {
                        if (asm != loadedAssembly) // Exclude the primary assembly
                        {
                            logger.LogDebug("Loaded dependent assembly. assembly={0}", asm.GetName());
                        }
                    }
                }

                // Register
                assemblies.Add(loadedAssembly);
            }

            return Task.FromResult(assemblies.AsEnumerable());
        }
    }
}