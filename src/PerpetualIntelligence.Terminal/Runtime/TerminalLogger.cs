/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using System;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// An abstraction of a <see cref="Terminal"/> logger.
    /// </summary>
    public abstract class TerminalLogger : ILogger
    {
        /// <inheritdoc/>
        public abstract IDisposable? BeginScope<TState>(TState state) where TState : notnull;

        /// <inheritdoc/>
        public abstract bool IsEnabled(LogLevel logLevel);

        /// <inheritdoc/>
        public abstract void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter);
    }
}