/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers
{
    /// <summary>
    /// The default <see cref="IExceptionHandler"/> to handle an <see cref="Exception"/>.
    /// </summary>
    public class ExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public ExceptionHandler(CliOptions options, ILogger<ExceptionHandler> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Publish the <see cref="Exception"/> asynchronously to the logger.
        /// </summary>
        /// <param name="context">The error to publish.</param>
        /// <returns>The string representation.</returns>
        public Task HandleAsync(ExceptionHandlerContext context)
        {
            if (context.Exception is ErrorException ee)
            {
                logger.FormatAndLog(LogLevel.Error, options.Logging, ee.Error.ErrorDescription ?? "", ee.Error.Args ?? Array.Empty<object?>());
            }
            else if (context.Exception is MultiErrorException me)
            {
                foreach (Error err in me.Errors)
                {
                    logger.FormatAndLog(LogLevel.Error, options.Logging, err.ErrorDescription ?? "", err.Args ?? Array.Empty<object?>());
                }
            }
            else if (context.Exception is OperationCanceledException oe)
            {
                logger.FormatAndLog(LogLevel.Error, options.Logging, "The request was canceled. command_string={0}", context.RawCommandString);
            }
            else
            {
                logger.FormatAndLog(LogLevel.Error, options.Logging, "The request failed. command_string={0} additional_info={1}", context.RawCommandString, context.Exception.Message);
            }

            return Task.CompletedTask;
        }

        private readonly ILogger<ExceptionHandler> logger;
        private readonly CliOptions options;
    }
}
