/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Collections.Generic;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ILogger"/> that indents  messages based on scopes to the <see cref="ITerminalConsole"/>.
    /// </summary>
    public sealed class TerminalConsoleLogger : TerminalLogger
    {
        private readonly TerminalOptions options;
        private readonly ITerminalConsole terminalConsole;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="name">The logger name.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="terminalConsole">The terminal console.</param>
        public TerminalConsoleLogger(string name, TerminalOptions options, ITerminalConsole terminalConsole)
        {
            Scopes = new List<IDisposable>();
            this.options = options;
            this.terminalConsole = terminalConsole;
        }

        /// <summary>
        /// The console logger scopes.
        /// </summary>
        public List<IDisposable> Scopes { get; }

        /// <summary>
        /// The standard application logger.
        /// </summary>
        public ILogger? Logger { get; }

        /// <inheritdoc/>
        public override IDisposable? BeginScope<TState>(TState state)
        {
            return new TerminalConsoleLoggerScope<TState>(state, this);
        }

        /// <inheritdoc/>
        public override bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc/>
        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            // Display the message to console
            if (Scopes.Count > 0)
            {
                terminalConsole.WriteLineAsync(new string(' ', Scopes.Count * options.Logging.LoggerIndent) + formatter.Invoke(state, exception)).Wait();
            }
            else
            {
                terminalConsole.WriteLineAsync(formatter.Invoke(state, exception)).Wait();
            }

            // Log to standard logger.
            if (Logger != null && options.Logging.LogToStandard.GetValueOrDefault())
            {
                Logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }
    }
}