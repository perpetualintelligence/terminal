/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using System;
using System.Collections.Concurrent;

namespace PerpetualIntelligence.Cli.Runtime
{
    /// <summary>
    /// The <see cref="ILoggerProvider"/> for <see cref="ITerminalLogger"/>.
    /// </summary>
    public sealed class TerminalLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ITerminalLogger> loggers;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public TerminalLoggerProvider()
        {
            loggers = new ConcurrentDictionary<string, ITerminalLogger>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates an instance of <see cref="TerminalConsoleLogger"/>.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            // Use configure to pass the pre-configured options
            return loggers.GetOrAdd(categoryName, factory => new TerminalConsoleLogger(categoryName, null!));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            loggers.Clear();
        }
    }
}