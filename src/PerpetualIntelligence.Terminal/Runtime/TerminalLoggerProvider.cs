/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace PerpetualIntelligence.Cli.Runtime
{
    /// <summary>
    /// The <see cref="ILoggerProvider"/> for <see cref="TerminalLogger"/>.
    /// </summary>
    public sealed class TerminalLoggerProvider<TLogger> : ILoggerProvider where TLogger : TerminalLogger
    {
        private readonly ConcurrentDictionary<string, TerminalLogger> loggers;
        private readonly IServiceProvider services;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public TerminalLoggerProvider(IServiceProvider services)
        {
            loggers = new ConcurrentDictionary<string, TerminalLogger>(StringComparer.OrdinalIgnoreCase);
            this.services = services;
        }

        /// <summary>
        /// Creates an instance of <see cref="TerminalConsoleLogger"/>.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            // Use configure to pass the pre-configured options
            return loggers.GetOrAdd(categoryName, factory => (TerminalLogger)ActivatorUtilities.CreateInstance(services, typeof(TLogger), categoryName));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            loggers.Clear();
        }
    }
}