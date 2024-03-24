/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// A no-operation console implementation for the <see cref="ITerminalConsole"/> interface.
    /// This implementation does not perform any actual console input/output operations.
    /// </summary>
    public class TerminalNoConsole : ITerminalConsole
    {
        /// <inheritdoc/>
        public ConsoleColor ForegroundColor { get; set; }

        /// <inheritdoc/>
        public ConsoleColor BackgroundColor { get; set; }

        /// <inheritdoc/>
        public Task ClearAsync()
        {
            // No operation
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public bool Ignore(string? value)
        {
            // Assume all values are ignored in this no-op implementation
            return true;
        }

        /// <inheritdoc/>
        public Task<string?> ReadLineAsync()
        {
            // Return null indicating no input
            return Task.FromResult<string?>(null);
        }

        /// <inheritdoc/>
        public Task<string> ReadAnswerAsync(string question, params string[]? answers)
        {
            // Return an empty string as the answer
            return Task.FromResult(string.Empty);
        }

        /// <inheritdoc/>
        public Task WriteLineAsync()
        {
            // No operation
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task WriteLineAsync(string value, params object[] args)
        {
            // No operation
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task WriteLineColorAsync(ConsoleColor foregroundColor, string value, params object[] args)
        {
            // No operation
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task WriteColorAsync(ConsoleColor foregroundColor, string value, params object[] args)
        {
            // No operation
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task WriteAsync(string value, params object[] args)
        {
            // No operation
            return Task.CompletedTask;
        }
    }
}