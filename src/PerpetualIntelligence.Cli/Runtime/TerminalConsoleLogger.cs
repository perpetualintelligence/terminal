/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using System;
using System.Collections.Generic;

namespace PerpetualIntelligence.Cli.Runtime
{
    /// <summary>
    /// The default <see cref="ILogger"/> that indents  messages based on scopes to the standard <see cref="Console"/>.
    /// </summary>
    public sealed class TerminalConsoleLogger : TerminalLogger
    {
        private readonly CliOptions options;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="name">The logger name.</param>
        /// <param name="options">The configuration options.</param>
        public TerminalConsoleLogger(string name, CliOptions options)
        {
            Scopes = new List<IDisposable>();
            this.options = options;
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
                Console.WriteLine(new string(' ', Scopes.Count * options.Logging.LoggerIndent) + formatter.Invoke(state, exception));
            }
            else
            {
                Console.WriteLine(formatter.Invoke(state, exception));
            }

            // Log to standard logger.
            if (Logger != null && options.Logging.LogToStandard.GetValueOrDefault())
            {
                Logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }
    }
}