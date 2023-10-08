/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Handlers
{
    /// <summary>
    /// The default <see cref="IExceptionHandler"/> to handle an <see cref="Exception"/>.
    /// </summary>
    public class ExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ExceptionHandler(ILogger<ExceptionHandler> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Publish the <see cref="Exception"/> asynchronously to the logger.
        /// </summary>
        /// <param name="context">The error to publish.</param>
        /// <returns>The string representation.</returns>
        public Task HandleExceptionAsync(ExceptionHandlerContext context)
        {
            if (context.Exception is TerminalException ee)
            {
                logger.LogError(ee, ee.Error.ErrorDescription, ee.Error.Args ?? Array.Empty<object?>());
            }
            else if (context.Exception is MultiErrorException me)
            {
                foreach (Error err in me.Errors)
                {
                    logger.LogError(me, err.ErrorDescription, err.Args ?? Array.Empty<object?>());
                }
            }
            else if (context.Exception is OperationCanceledException oe)
            {
                if (context.CommandRoute != null)
                {
                    logger.LogError(oe, "The request was canceled. route={0} command={1}", context.CommandRoute.Id, context.CommandRoute.Command.Raw);
                }
                else
                {
                    logger.LogError(oe, "The request was canceled.");
                }
            }
            else
            {
                if (context.CommandRoute != null)
                {
                    logger.LogError(context.Exception, "The request failed. route={0} command={1}", context.CommandRoute.Id, context.CommandRoute.Command.Raw);
                }
                else
                {
                    logger.LogError(context.Exception, "The request failed.");
                }
            }

            return Task.CompletedTask;
        }

        private readonly ILogger<ExceptionHandler> logger;
    }
}