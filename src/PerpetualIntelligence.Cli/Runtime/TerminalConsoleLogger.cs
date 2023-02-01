/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using System;
using System.Collections.Generic;

namespace PerpetualIntelligence.Cli.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalLogger"/> that indents  messages based on scopes to the standard <see cref="Console"/>.
    /// </summary>
    public sealed class TerminalConsoleLogger : ITerminalLogger
    {
        private readonly CliOptions cliOptions;

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public TerminalConsoleLogger(CliOptions cliOptions)
        {
            Scopes = new List<IDisposable>();
            this.cliOptions = cliOptions;
        }

        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return new TerminalConsoleLoggerScope<TState>(state, this);
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (Scopes.Count > 0)
            {
                Console.WriteLine(new string(' ', Scopes.Count * cliOptions.Terminal.LoggerIndent) + formatter.Invoke(state, exception));
            }
            else
            {
                Console.WriteLine(formatter.Invoke(state, exception));
            }
        }

        /// <summary>
        /// The console logger scopes.
        /// </summary>
        public List<IDisposable> Scopes { get; }
    }
}