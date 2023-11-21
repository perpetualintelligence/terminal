/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Authentication
{
    /// <summary>
    /// An abstraction of a client's cross platform token cache. Client applications (desktop and mobile apps) should
    /// try to get a token from the cache before acquiring a token by another method.
    /// </summary>
    /// <seealso cref="MsalCacheHelper"/>
    /// <seealso href="https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-token-cache-serialization?tabs=desktop"/>
    /// <seealso href="https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet/wiki/Cross-platform-Token-Cache"/>
    /// <seealso href="https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet/blob/master/sample/ManualTestApp/Program.cs"/>
    public interface IClientCrossPlatformTokenCache
    {
        /// <summary>
        /// Registers the cache with the client application.
        /// </summary>
        /// <param name="clientApplicationBase">The client application.</param>
        public Task RegisterCacheAsync(IClientApplicationBase clientApplicationBase);
    }
}
