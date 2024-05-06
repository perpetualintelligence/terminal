/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Commands.Handlers;

namespace OneImlx.Terminal.Mocks
{
    internal class MockCommandAuthenticatorInner : ICommandAuthenticator
    {
        public Task AuthenticateAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task AuthorizeAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> IsAuthenticatedAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
