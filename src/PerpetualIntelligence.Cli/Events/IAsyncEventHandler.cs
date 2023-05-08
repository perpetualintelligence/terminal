/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Commands.Runners;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Events
{
    /// <summary>
    /// The <c>pi-cli</c> asynchronous event handler.
    /// </summary>
    public interface IAsyncEventHandler
    {
        /// <summary>
        /// Override this method if you will perform an asynchronous operation before <see cref="ICommandRouter"/> starts a command route.
        /// </summary>
        /// <param name="commandRoute">The command route.</param>
        public Task BeforeCommandRouteAsync(CommandRoute commandRoute);

        /// <summary>
        /// Override this method if you will perform an asynchronous operation after <see cref="ICommandRouter"/> ends a command route and process the command result.
        /// </summary>
        /// <param name="command">The command object.</param>
        /// <param name="result">The command router result.</param>
        /// <remarks>
        /// The framework will call <see cref="AfterCommandRouteAsync(Command?, CommandRouterResult?)"/> even if there is an error during command routing.
        /// </remarks>
        public Task AfterCommandRouteAsync(Command? command, CommandRouterResult? result);

        /// <summary>
        /// Override this method if you will perform an asynchronous operation before <see cref="ICommandHandler"/> starts a command run.
        /// </summary>
        /// <param name="command">The command object.</param>
        public Task BeforeCommandRunAsync(Command command);

        /// <summary>
        /// Override this method if you will perform an asynchronous operation after <see cref="ICommandHandler"/> ends a command run and process the command result.
        /// </summary>
        /// <param name="command">The command object.</param>
        /// <param name="result">The command run result.</param>
        /// <remarks>
        /// The framework will not call <see cref="AfterCommandRunAsync(Command, CommandRunnerResult)"/> if there is an error during the command run.
        /// </remarks>
        public Task AfterCommandRunAsync(Command command, CommandRunnerResult result);

        /// <summary>
        /// Override this method if you will perform an asynchronous operation before <see cref="ICommandHandler"/> starts a command check.
        /// </summary>
        /// <param name="command">The command object.</param>
        public Task BeforeCommandCheckAsync(Command command);

        /// <summary>
        /// Override this method if you will perform an asynchronous operation after <see cref="ICommandHandler"/> ends a command check.
        /// </summary>
        /// <param name="command">The command object.</param>
        /// <param name="result">The command run result.</param>
        /// <remarks>
        /// The framework will not call <see cref="AfterCommandCheckAsync(Command, CommandCheckerResult)"/> if there is an error during the command check.
        /// </remarks>
        public Task AfterCommandCheckAsync(Command command, CommandCheckerResult result);
    }
}