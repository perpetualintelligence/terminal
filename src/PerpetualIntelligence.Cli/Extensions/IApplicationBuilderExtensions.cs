/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.AspNetCore.Builder;
using PerpetualIntelligence.Cli.Middlewares;

namespace PerpetualIntelligence.Cli.Extensions
{
    /// <summary>
    /// Pipeline extension methods for adding Perpetual Intelligence identity.
    /// </summary>
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds IdentityServer to the pipeline.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseCli(this IApplicationBuilder app)
        {
            app.UseMiddleware<OneImlxCliMiddleware>();
            return app;
        }
    }
}
