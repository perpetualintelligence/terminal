/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Linq;

namespace PerpetualIntelligence.Terminal.Extensions
{
    /// <summary>
    /// The <see cref="ILogger{T}"/> extension methods.
    /// </summary>
    public static class ILoggerExtensions
    {
        /// <summary>
        /// Logs the formated message and returns the logged message for downstream processing.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="logLevel"></param>
        /// <param name="loggingOptions"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string FormatAndLog(this ILogger logger, LogLevel logLevel, LoggingOptions loggingOptions, string message, params object?[] args)
        {
            // For downstream processing
            Tuple<string, object?[]> formatted = FormatMessage(loggingOptions, message, args);

            // For actual logging
            switch (logLevel)
            {
                case LogLevel.Information:
                    {
                        logger.LogInformation(message, formatted.Item2);
                        break;
                    }
                case LogLevel.Warning:
                    {
                        logger.LogWarning(message, formatted.Item2);
                        break;
                    }
                case LogLevel.Error:
                    {
                        logger.LogError(message, formatted.Item2);
                        break;
                    }
                case LogLevel.Debug:
                    {
                        logger.LogDebug(message, formatted.Item2);
                        break;
                    }
                case LogLevel.Trace:
                    {
                        logger.LogTrace(message, formatted.Item2);
                        break;
                    }
                case LogLevel.Critical:
                    {
                        logger.LogCritical(message, formatted.Item2);
                        break;
                    }
                default:
                    {
                        // Don't log
                        break;
                    }
            }

            return formatted.Item1;
        }

        private static Tuple<string, object?[]> FormatMessage(LoggingOptions loggingOptions, string message, params object?[] args)
        {
            if (args != null)
            {
                object?[] argsToUse = loggingOptions.ObsureInvalidOptions ? Obscure(loggingOptions.ObscureStringForInvalidOption, args) : args;
                return new(string.Format(message, argsToUse), argsToUse);
            }
            else
            {
                return new(message, Array.Empty<object?>()); ;
            }
        }

        /// <summary>
        /// Formats the error message for downstream processing.
        /// </summary>
        /// <param name="loggingOptions">The logging options. See <see cref="LoggingOptions"/>.</param>
        /// <param name="message">The message to format.</param>
        /// <param name="args">The format arguments.</param>
        /// <returns>The formatted error message.</returns>
        private static string Format(LoggingOptions loggingOptions, string message, params object?[] args)
        {
            return string.Format(message, loggingOptions.ObsureInvalidOptions ? Obscure(loggingOptions.ObscureStringForInvalidOption, args) : args);
        }

        /// <summary>
        /// Obscures the arguments based on the specified mask.
        /// </summary>
        /// <param name="mask">The obscure mask.</param>
        /// <param name="args">The arguments to obscure.</param>
        /// <returns>The obscured arguments.</returns>
        private static object?[] Obscure(string mask, params object?[] args)
        {
            object?[] argsToUse = args;
            if (args != null)
            {
                argsToUse = Enumerable.Repeat(mask, args.Length).ToArray();
            }
            return argsToUse;
        }
    }
}