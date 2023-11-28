﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/
/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Test.Services;
using Xunit;

namespace PerpetualIntelligence.Terminal
{
    public class AssemblyTests
    {
        [Fact(Skip = "Fix Test")]
        public void TypesNamespaceTest()
        {
            TestHelper.AssertNamespace(typeof(TerminalErrors).Assembly, "PerpetualIntelligence.Terminal");
        }

        [Fact(Skip = "Fix Test")]
        public void TypesLocationTest()
        {
            TestHelper.AssertAssemblyTypesLocation(typeof(TerminalErrors).Assembly);
        }
    }
}