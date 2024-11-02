/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OneImlx.Terminal.AspNetCore.Extensions
{
    /// <summary>
    /// The <see cref="IEndpointRouteBuilder"/> extension methods.
    /// </summary>
    public static class EndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Maps the terminal HTTP commands endpoint to handle incoming HTTP requests for the terminal server.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to configure routing for terminal commands.</param>
        /// <returns>The <see cref="IEndpointConventionBuilder"/> for the mapped endpoint.</returns>
        /// <remarks>
        /// This method registers a POST endpoint at <c>/oneimlx/terminal/httprouter</c> that integrates with the
        /// <see cref="TerminalHttpMapService"/> to handle command requests.
        /// </remarks>
        public static IEndpointConventionBuilder MapTerminalHttp(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapPost("/oneimlx/terminal/httprouter", async context =>
            {
                // Resolve TerminalHttpMapService from DI and process the command
                var terminalHttpMapService = context.RequestServices.GetRequiredService<TerminalHttpMapService>();

                // Route the command received from the HTTP request to the terminal server. The command gets added to
                // the command queue and is processed by the terminal server's command loop.
                await terminalHttpMapService.RouteCommandAsync(context);
            });
        }
    }
}
