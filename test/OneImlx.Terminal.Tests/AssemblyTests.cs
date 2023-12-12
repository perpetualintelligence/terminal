/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Test.Services;
using Xunit;

namespace OneImlx.Terminal
{
    public class AssemblyTests
    {
        [Fact]
        public void TypesNamespaceTest()
        {
            TestHelper.AssertNamespace(typeof(TerminalErrors).Assembly, "OneImlx.Terminal");
        }

        [Fact]
        public void TypesLocationTest()
        {
            TestHelper.AssertAssemblyTypesLocation(typeof(TerminalErrors).Assembly);
        }
    }
}