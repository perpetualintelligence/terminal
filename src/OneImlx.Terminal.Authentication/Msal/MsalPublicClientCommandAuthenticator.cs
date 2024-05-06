/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Configuration.Options;

namespace OneImlx.Terminal.Authentication.Msal
{
    public sealed class MsalPublicClientCommandAuthenticator : ICommandAuthenticator
    {
        private readonly IPublicClientApplication publicClientApplication;
        private readonly TerminalOptions options;
        private readonly ILogger<MsalPublicClientTokenAcquisition> logger;

        public MsalPublicClientCommandAuthenticator(
            IPublicClientApplication publicClientApplication,
            TerminalOptions options,
            ILogger<MsalPublicClientTokenAcquisition> logger)
        {
            this.publicClientApplication = publicClientApplication;
            this.options = options;
            this.logger = logger;
        }

        public Task AuthenticateAsync()
        {
            throw new NotImplementedException();
        }

        public Task AuthorizeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsAuthenticatedAsync()
        {
            throw new NotImplementedException();
        }
    }
}
