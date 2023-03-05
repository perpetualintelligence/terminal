/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Runtime;

namespace PerpetualIntelligence.Cli.Extensions
{
    /// <summary>
    /// <see cref="ILoggingBuilder"/> extension methods.
    /// </summary>
    public static class ILoggingBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="TerminalLoggerProvider"/> to the logging providers.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ILoggingBuilder AddTerminalLogger<TTerminalLogger>(this ILoggingBuilder builder) where TTerminalLogger : class, ITerminalLogger
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TerminalLoggerProvider>());
            return builder;
        }
    }
}