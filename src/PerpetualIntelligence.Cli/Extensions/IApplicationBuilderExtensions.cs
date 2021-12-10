/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.AspNetCore.Builder;

namespace PerpetualIntelligence.Cli.Extensions
{
    /// <summary>
    /// The <see cref="IApplicationBuilder"/> extension methods.
    /// </summary>
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds middleware for using Perpetual Intelligence's <c>cli</c> framework.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseCli(this IApplicationBuilder app)
        {
            // TODO
            return app;
        }
    }
}
