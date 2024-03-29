﻿/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Security.Claims;

namespace OneImlx.Terminal.Mocks
{
    public class MockClaimsPrincipal
    {
        public static ClaimsPrincipal NewEmpty()
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}
