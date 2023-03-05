/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Runners;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Events
{
    /// <summary>
    /// The  <c>pi-cli</c> asynchronous event handler.
    /// </summary>
    public interface IAsyncCliEventHandler
    {
        /// <summary>
        /// Override this method if you will perform an asynchronous operation before <see cref="ICommandHandler"/> starts a command run.
        /// </summary>
        /// <param name="command">The command object.</param>
        public Task BeforeCommandRunAsync(Command command);

        /// <summary>
        /// Override this method if you will perform an asynchronous operation after <see cref="ICommandHandler"/> ends a command run and process the command result..
        /// </summary>
        /// <param name="command">The command object.</param>
        /// <param name="result">The command run result.</param>
        public Task AfterCommandRunAsync(Command command, CommandRunnerResult result);

        /// <summary>
        /// Override this method if you will perform an asynchronous operation before <see cref="ICommandHandler"/> starts a command check.
        /// </summary>
        /// <param name="commandDescriptor">The command descriptor object.</param>
        /// <param name="command">The command object.</param>
        public Task BeforeCommandCheckAsync(CommandDescriptor commandDescriptor, Command command);

        /// <summary>
        /// Override this method if you will perform an asynchronous operation after <see cref="ICommandHandler"/> ends a command check.
        /// </summary>
        /// <param name="commandDescriptor">The command descriptor object.</param>
        /// <param name="command">The command object.</param>
        /// <param name="result">The command run result.</param>
        public Task AfterCommandCheckAsync(CommandDescriptor commandDescriptor, Command command, CommandCheckerResult result);
    }
}