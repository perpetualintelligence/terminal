/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Shared.Infrastructure;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalExceptionHandler"/> to handle an <see cref="Exception"/>.
    /// </summary>
    public class TerminalExceptionHandler : ITerminalExceptionHandler
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public TerminalExceptionHandler(ILogger<TerminalExceptionHandler> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Publish the <see cref="Exception"/> asynchronously to the logger.
        /// </summary>
        /// <param name="context">The error to publish.</param>
        /// <returns>The string representation.</returns>
        public Task HandleExceptionAsync(TerminalExceptionHandlerContext context)
        {
            logger.LogDebug("Handle exception. route={0}", context.Route != null ? context.Route.Id : "<null>");

            if (context.Exception is TerminalException ee)
            {
                logger.LogError(ee.Error.ErrorDescription, ee.Error.Args ?? Array.Empty<object?>());
            }
            else if (context.Exception is MultiErrorException me)
            {
                foreach (Error err in me.Errors)
                {
                    logger.LogError(err.ErrorDescription, err.Args ?? Array.Empty<object?>());
                }
            }
            else if (context.Exception is OperationCanceledException oe)
            {
                if (context.Route != null)
                {
                    logger.LogError("The request was canceled. route={0} command={1}", context.Route.Id, context.Route.Raw);
                }
                else
                {
                    logger.LogError("The request was canceled.");
                }
            }
            else
            {
                if (context.Route != null)
                {
                    logger.LogError("The request failed. route={0} command={1} info={2}", context.Route.Id, context.Route.Raw, context.Exception.Message);
                }
                else
                {
                    logger.LogError("The request failed.");
                }
            }

            return Task.CompletedTask;
        }

        private readonly ILogger<TerminalExceptionHandler> logger;
    }
}