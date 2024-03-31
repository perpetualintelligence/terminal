/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Runners;
using System;

namespace OneImlx.Terminal.Commands.Handlers
{
    /// <summary>
    /// The default <see cref="ICommandRuntime"/> using <see cref="ActivatorUtilities.CreateInstance(IServiceProvider, Type, object[])"/>, managing the resolution of
    /// of command checkers and runners.
    /// </summary>
    public sealed class CommandRuntime : ICommandRuntime
    {
        private readonly IServiceProvider serviceDescriptors;
        private readonly ILogger<CommandRuntime> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRuntime"/> class.
        /// </summary>
        /// <param name="serviceDescriptors">The service provider for resolving service instances.</param>
        /// <param name="logger">The logger for logging diagnostic messages.</param>
        public CommandRuntime(IServiceProvider serviceDescriptors, ILogger<CommandRuntime> logger)
        {
            this.serviceDescriptors = serviceDescriptors;
            this.logger = logger;
        }

        /// <summary>
        /// Resolves the command checker for a given command descriptor using dependency injection and activator utilities.
        /// </summary>
        /// <param name="commandDescriptor">The command descriptor to identify the checker.</param>
        /// <returns>The resolved command checker.</returns>
        /// <exception cref="TerminalException">Thrown if the command checker is not configured or invalid.</exception>
        public ICommandChecker ResolveCommandChecker(CommandDescriptor commandDescriptor)
        {
            if (commandDescriptor.Checker == null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The command checker is not configured. command={0}", commandDescriptor.Id);
            }

            object? checkerObj = ActivatorUtilities.CreateInstance(serviceDescriptors, commandDescriptor.Checker);

            if (checkerObj is not ICommandChecker checker)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The command checker is not valid. command={0} checker={1}", commandDescriptor.Id, commandDescriptor.Checker.Name);
            }

            logger.LogDebug("Resolved checker. type={0}", checker.GetType().Name);
            return checker;
        }

        /// <summary>
        /// Resolves the command runner for a given command descriptor using dependency injection and activator utilities.
        /// </summary>
        /// <param name="commandDescriptor">The command descriptor to identify the runner.</param>
        /// <returns>The resolved command runner.</returns>
        /// <exception cref="TerminalException">Thrown if the command runner is not configured or invalid.</exception>
        public IDelegateCommandRunner ResolveCommandRunner(CommandDescriptor commandDescriptor)
        {
            if (commandDescriptor.Runner == null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The command runner delegate is not configured. command={0}", commandDescriptor.Id);
            }

            object? runnerObj = ActivatorUtilities.CreateInstance(serviceDescriptors, commandDescriptor.Runner);

            if (runnerObj is not IDelegateCommandRunner runnerDelegate)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The command runner delegate is not valid. command={0} runner={1}", commandDescriptor.Id, commandDescriptor.Runner.Name);
            }

            logger.LogDebug("Resolved runner. type={0}", runnerDelegate.GetType().Name);
            return runnerDelegate;
        }
    }
}