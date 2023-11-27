/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Authentication.Msal
{
    /// <summary>
    /// Provides an abstraction for managing a cross-platform token cache in client applications, such as desktop and mobile apps.
    /// This interface allows applications to interact with a token cache, facilitating efficient and secure token retrieval and storage.
    /// Implementations should manage token caching to optimize authentication flows by reducing the need for frequent token acquisition.
    /// </summary>
    /// <remarks>
    /// Implementations should ensure secure storage and handling of sensitive token data, complying with platform-specific security standards.
    /// Refer to the MSAL documentation and the Microsoft Authentication Extensions for .NET for guidance on implementing cross-platform token caching.
    /// </remarks>
    /// <seealso cref="MsalCacheHelper"/>
    /// <seealso href="https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-token-cache-serialization?tabs=desktop"/>
    /// <seealso href="https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet/wiki/Cross-platform-Token-Cache"/>
    /// <seealso href="https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet/blob/master/sample/ManualTestApp/Program.cs"/>
    public interface IMsalTokenCache
    {
        /// <summary>
        /// Registers the token cache with a client application, enabling the application to utilize the cache for token operations.
        /// This method should be called during the initialization of the client application to ensure that the token cache is properly set up.
        /// </summary>
        /// <param name="clientApplicationBase">The client application with which the token cache will be associated.</param>
        /// <returns>A task representing the asynchronous operation of cache registration.</returns>
        public Task RegisterCacheAsync(IClientApplicationBase clientApplicationBase);
    }
}