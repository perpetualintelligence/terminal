/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Runners
{
    /// <summary>
    /// The command runner result.
    /// </summary>
    public class CommandRunnerResult : IAsyncDisposable
    {
        /// <summary>
        /// Determines whether the result is disposed.
        /// </summary>
        public bool IsDisposed { get; protected set; }

        /// <summary>
        /// Determines whether the result is processed.
        /// </summary>
        public bool IsProcessed { get; protected set; }

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
        /// <remarks>
        /// Derived implementation should call base class implementation to mark the result as processed, see <see cref="IsProcessed"/>.
        /// </remarks>
        public virtual Task ProcessAsync(CommandRunnerResultProcessorContext context)
        {
            IsProcessed = true;
            return Task.CompletedTask;
        }
    }
}