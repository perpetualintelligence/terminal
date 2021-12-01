/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Middlewares
{
    /// <summary>
    /// The <c>oneimlx</c> cli middleware.
    /// </summary>
    public sealed class OneImlxCliMiddleware
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OneImlxCliMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware.</param>
        /// <param name="logger">The logger.</param>
        public OneImlxCliMiddleware(RequestDelegate next, ILogger<OneImlxCliMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="router">The command router.</param>
        /// <returns></returns>
        public async Task Invoke(
            HttpContext context,
            ICommandRouter router)
        {
            try
            {
                // Setup the request context for route
                CommandContext ccontext = new("tbd", context.RequestServices);

                // Route the request and process the result. If the request is not routed then fall through to the next
                // in pipeline.
                CommandResult routed = await router.RouteAsync(ccontext);
                if (!routed.IsError)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unknown OneImlx cli middleware exception. Exception={0}", ex.Message);
                throw;
            }

            // Fall-through to next pipeline request.
            await _next(context);
        }

        private readonly ILogger _logger;
        private readonly RequestDelegate _next;
    }
}
