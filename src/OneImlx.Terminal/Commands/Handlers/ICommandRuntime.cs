/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Runners;

namespace OneImlx.Terminal.Commands.Handlers
{
    /// <summary>
    /// An abstraction for resolving command runtime. It is responsible for resolving the appropriate command checker
    /// and runner for a given command descriptor.
    /// </summary>
    /// <seealso cref="CommandRuntime"/>
    public interface ICommandRuntime
    {
        /// <summary>
        /// Resolves the command authenticator associated with the specified command descriptor.
        /// </summary>
        /// <param name="commandDescriptor">The descriptor of the command for which to resolve the checker.</param>
        /// <returns>The resolved command authenticator instance.</returns>
        ICommandAuthenticator ResolveCommandAuthenticator(CommandDescriptor commandDescriptor);

        /// <summary>
        /// Resolves the command checker associated with the specified command descriptor.
        /// </summary>
        /// <param name="commandDescriptor">The descriptor of the command for which to resolve the checker.</param>
        /// <returns>The resolved command checker instance.</returns>
        ICommandChecker ResolveCommandChecker(CommandDescriptor commandDescriptor);

        /// <summary>
        /// Resolves the command runner associated with the specified command descriptor.
        /// </summary>
        /// <param name="commandDescriptor">The descriptor of the command for which to resolve the runner.</param>
        /// <returns>The resolved command runner instance.</returns>
        IDelegateCommandRunner ResolveCommandRunner(CommandDescriptor commandDescriptor);
    }
}
