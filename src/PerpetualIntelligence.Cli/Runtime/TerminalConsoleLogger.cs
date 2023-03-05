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
    /// The default <see cref="ITerminalLogger{TCategoryName}"/> that indents  messages based on scopes to the standard <see cref="Console"/>.
    /// </summary>
    public sealed class TerminalConsoleLogger<TCategoryName> : ITerminalLogger<TCategoryName>
    {
        private readonly CliOptions options;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The standard logger.</param>
        public TerminalConsoleLogger(CliOptions options, ILogger<TCategoryName> logger)
        {
            Scopes = new List<IDisposable>();
            this.options = options;
            Logger = logger;
        }

        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return new TerminalConsoleLoggerScope<TCategoryName, TState>(state, this);
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            // Display the message to console
            if (Scopes.Count > 0)
            {
                Console.WriteLine(new string(' ', Scopes.Count * options.Terminal.LoggerIndent) + formatter.Invoke(state, exception));
            }
            else
            {
                Console.WriteLine(formatter.Invoke(state, exception));
            }

            // Log to standard logger.
            if (options.Terminal.LogToStandard.GetValueOrDefault())
            {
                Logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }

        /// <summary>
        /// The console logger scopes.
        /// </summary>
        public List<IDisposable> Scopes { get; }

        /// <summary>
        /// The standard application logger.
        /// </summary>
        public ILogger<TCategoryName> Logger { get; }
    }
}