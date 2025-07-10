/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Duende.IdentityModel.OidcClient;
using OneImlx.Terminal.Authentication;
using OneImlx.Terminal.Authentication.Oidc;
using System.Security.Claims;

/// <summary>
/// OIDC token acquisition implementation using OidcClient (interactive + silent).
/// </summary>
public sealed class OidcKiotaTokenAcquisition : ITokenAcquisition
{
    public OidcKiotaTokenAcquisition(OidcClient oidcClient, IOidcUserTokenStore userTokenStore, ILogger<OidcKiotaTokenAcquisition> logger)
    {
        this.oidcClient = oidcClient;
        this.userTokenStore = userTokenStore;
        this.logger = logger;
    }

    public async Task<TokenAcquisitionResult> AcquireTokenInteractiveAsync(TokenAcquisitionInteractiveInput input)
    {
        var result = await oidcClient.LoginAsync(new LoginRequest());
        if (result.IsError)
        {
            throw new InvalidOperationException($"OIDC interactive login failed. {result.Error}");
        }

        ClaimsPrincipal user = new();
        await userTokenStore.StoreTokenAsync(user, new TokenAcquisitionResult()); // key could be refined for multi-user

        return new TokenAcquisitionResult
        {
            AccessToken = result.AccessToken,
            IdToken = result.IdentityToken,
            Claims = result.User?.Claims
        };
    }

    public async Task<TokenAcquisitionResult> AcquireTokenSilentAsync(TokenAcquisitionSilentInput input)
    {
        ClaimsPrincipal user = new();
        var result = await userTokenStore.GetTokenAsync(user);
        if (result == null || string.IsNullOrEmpty(result.AccessToken))
        {
            throw new InvalidOperationException("No cached OIDC token available for silent authentication.");
        }

        return new TokenAcquisitionResult
        {
            AccessToken = result.AccessToken,
            IdToken = result.IdToken,
            Claims = result.Claims
        };
    }

    public Task<IEnumerable<TokenAcquisitionIdentity>> GetIdentitiesAsync(string? userFlow = null)
    {
        return Task.FromResult<IEnumerable<TokenAcquisitionIdentity>>(new[]
        {
            new TokenAcquisitionIdentity { Raw = "oidc-session" }
        });
    }

    private readonly ILogger<OidcKiotaTokenAcquisition> logger;
    private readonly OidcClient oidcClient;
    private readonly IOidcUserTokenStore userTokenStore;
}
