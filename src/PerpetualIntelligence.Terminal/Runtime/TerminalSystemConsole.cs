/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalConsole"/> implementation that use system <see cref="Console"/>.
    /// </summary>
    public sealed class TerminalSystemConsole : ITerminalConsole
    {
        /// <inheritdoc/>
        public bool Ignore(string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <inheritdoc/>
        public Task<string?> ReadLineAsync()
        {
            return Task.Run(() => { return (string?)Console.ReadLine(); });
        }

        /// <inheritdoc/>
        public Task WriteAsync(string value)
        {
            return Task.Run(() => { Console.Write(value); });
        }
    }
}