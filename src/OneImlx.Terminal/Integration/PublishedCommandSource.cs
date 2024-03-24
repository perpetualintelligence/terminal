/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Integration
{
    /// <summary>
    /// The default implementation of <see cref="ITerminalCommandSource{TContext}"/> that loads terminal commands from assemblies in a published location.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is designed to dynamically load assemblies containing terminal commands. It is capable of handling both local and network-based published locations.
    /// The dynamic loading of assemblies allows for a modular and extensible design where terminal commands can be updated or extended without modifying the core application.
    /// </para>
    /// <para>
    /// Security Considerations:
    /// Since assemblies are loaded dynamically, it's critical to ensure they are from trusted sources. The class leverages <see cref="ITerminalCommandSourceChecker{TContext}"/>
    /// to validate and verify the integrity and authenticity of the assemblies before loading. Developers should implement robust checking mechanisms in the
    /// <see cref="ITerminalCommandSourceChecker{TContext}.CheckSourceAsync(TContext)"/> to mitigate risks such as code injection or running untrusted code.
    /// </para>
    /// <para>
    /// Compatibility Concerns:
    /// Care should be taken to ensure compatibility of the dynamically loaded assemblies with the main application, particularly in terms of versioning and dependency management.
    /// </para>
    /// <para>
    /// Performance Implications:
    /// Dynamically loading assemblies can have performance implications, especially when loading from network locations. It's advisable to monitor performance and optimize
    /// the loading process.
    /// </para>
    /// </remarks>
    public class PublishedCommandSource : ITerminalCommandSource<PublishedCommandSourceContext>
    {
        private readonly ITerminalTextHandler textHandler;
        private readonly ITerminalCommandSourceAssemblyLoader<PublishedCommandSourceContext> assemblyLoader;
        private readonly ITerminalCommandSourceChecker<PublishedCommandSourceContext> terminalCommandSourceChecker;
        private readonly IMutableCommandStore commandStore;
        private readonly ILogger<PublishedCommandSource> logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="publishedAssembliesLoader">The published assemblies loader.</param>
        /// <param name="publishedCommandSourceChecker">The terminal source checker.</param>
        /// <param name="commandStore">The mutable command store.</param>
        /// <param name="logger">The logger.</param>
        public PublishedCommandSource(
            ITerminalTextHandler textHandler,
            ITerminalCommandSourceAssemblyLoader<PublishedCommandSourceContext> publishedAssembliesLoader,
            ITerminalCommandSourceChecker<PublishedCommandSourceContext> publishedCommandSourceChecker,
            IMutableCommandStore commandStore,
            ILogger<PublishedCommandSource> logger)
        {
            this.textHandler = textHandler;
            this.assemblyLoader = publishedAssembliesLoader;
            this.terminalCommandSourceChecker = publishedCommandSourceChecker;
            this.commandStore = commandStore;
            this.logger = logger;
        }

        /// <summary>
        /// Loads the assemblies from the <see cref="PublishedCommandSourceContext.PublishedAssemblies"/>.
        /// </summary>
        /// <remarks>
        /// The published location can be a local location or a remote location on a network. When loading assemblies dynamically, be aware of security
        /// implications and compatibility issues, especially if you're loading third-party or untrusted assemblies. Always validate and verify the assemblies before
        /// loading them into your application context. The <see cref="LoadCommandSourceAsync(PublishedCommandSourceContext)"/> will call <see cref="ITerminalCommandSourceChecker{TContext}.CheckSourceAsync(TContext)"/>
        /// before it starts loading any assembly.
        /// </remarks>
        public async Task LoadCommandSourceAsync(PublishedCommandSourceContext context)
        {
            // Checks the terminal source.
            logger.LogInformation("Check published command source. type={0}", terminalCommandSourceChecker.GetType().Name);
            await terminalCommandSourceChecker.CheckSourceAsync(context);

            // Load assemblies
            logger.LogInformation("Load published command source. type={0} loader={1}", GetType().Name, assemblyLoader.GetType().Name);
            IEnumerable<Assembly> assemblies = await assemblyLoader.LoadAssembliesAsync(context);

            // Load all the published assemblies
            foreach (Assembly assembly in assemblies)
            {
                // Load the command runners, we create a temporary ITerminalBuilder and build the
                // command descriptor.
                logger.LogInformation("Load command runners. assembly={0}", assembly.GetName());
                ServiceCollection localServiceCollection = new();
                ITerminalBuilder terminalBuilder = localServiceCollection.CreateTerminalBuilder(textHandler);
                terminalBuilder.AddDeclarativeAssembly(assembly);
                ServiceProvider serviceProvider = localServiceCollection.BuildServiceProvider();
                IEnumerable<CommandDescriptor> commands = serviceProvider.GetRequiredService<IEnumerable<CommandDescriptor>>();
                foreach (var command in commands)
                {
                    bool added = await commandStore.TryAddAsync(command.Id, command);
                    if (added)
                    {
                        logger.LogTrace("Added command. command={0}", command.Id);
                    }
                    else
                    {
                        logger.LogWarning("Skipped command, already added. command={0}", command.Id);
                    }
                }
            }
        }
    }
}