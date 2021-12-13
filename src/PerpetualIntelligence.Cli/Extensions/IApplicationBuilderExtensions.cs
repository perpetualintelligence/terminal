/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.AspNetCore.Builder;
using System;

namespace PerpetualIntelligence.Cli.Extensions
{
    /// <summary>
    /// The <see cref="Console"/> extension methods.
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
