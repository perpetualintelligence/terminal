/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Abstractions;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Runners
{
    /// <summary>
    /// The command runner result.
    /// </summary>
    public class CommandRunnerResult : IProcessorNoResult<CommandRunnerResultProcessorContext>, IAsyncDisposable
    {
        /// <summary>
        /// Determines whether the result is disposed.
        /// </summary>
        public bool IsDisposed { get; protected set; }

        /// <summary>
        /// Creates a default runner result that does not perform any additional processing.
        /// </summary>
        public static CommandRunnerResult NoProcessing => new();

        /// <summary>
        /// Disposes the managed resources.
        /// </summary>
        /// <remarks>
        /// Derived implementation should call base class implementation to mark the result as disposed, see <see cref="IsDisposed"/>.
        /// </remarks>
        public virtual ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return new ValueTask();
        }

        /// <summary>
        /// Processes the runner result asynchronously.
        /// </summary>
        /// <param name="context">The runner result context.</param>
        /// <returns>An asynchronous task.</returns>
        public virtual Task ProcessAsync(CommandRunnerResultProcessorContext context)
        {
            return Task.CompletedTask;
        }
    }
}