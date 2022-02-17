/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Security.Claims;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockClaimsPrincipal
    {
        public static ClaimsPrincipal NewEmpty()
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}
