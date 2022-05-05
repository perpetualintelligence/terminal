﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Authentication
{
    /// <summary>
    /// The <see cref="IClientCrossPlatformTokenCache"/> that represents no caching.
    /// </summary>
    public class ClientCrossPlatformNoTokenCache : IClientCrossPlatformTokenCache
    {
        /// <summary>
        /// This method does nothing and it is provided for infrastructure consistency.
        /// </summary>
        /// <param name="clientApplicationBase"></param>
        /// <returns></returns>
        public Task RegisterCacheAsync(IClientApplicationBase clientApplicationBase)
        {
            return Task.CompletedTask;
        }
    }
}