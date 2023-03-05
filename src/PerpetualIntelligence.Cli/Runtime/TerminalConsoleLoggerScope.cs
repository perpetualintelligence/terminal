/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Runtime
{
    /// <summary>
    /// The <see cref="ITerminalLogger{TCategoryName}"/> scope.
    /// </summary>
    public sealed class TerminalConsoleLoggerScope<TCategoryName, TState> : IDisposable
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="state">The state identifier.</param>
        /// <param name="terminalConsoleLogger">The terminal console logger.</param>
        public TerminalConsoleLoggerScope(TState state, TerminalConsoleLogger<TCategoryName> terminalConsoleLogger)
        {
            State = state;
            TerminalConsoleLogger = terminalConsoleLogger;
            TerminalConsoleLogger.Scopes.Add(this);
        }

        /// <summary>
        /// The state identifier.
        /// </summary>
        public TState State { get; }

        /// <summary>
        /// The console logger.
        /// </summary>
        public TerminalConsoleLogger<TCategoryName> TerminalConsoleLogger { get; }

        /// <summary>
        /// Disposes the console scope.
        /// </summary>
        public void Dispose()
        {
            TerminalConsoleLogger.Scopes.Remove(this);
        }
    }
}