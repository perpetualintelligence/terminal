/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/
/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Test.Services;
using Xunit;

namespace OneImlx.Terminal.Authentication
{
    public class AssemblyTests
    {
        [Fact]
        public void TypesNamespaceTest()
        {
            TestHelper.AssertNamespace(typeof(Extensions.ITerminalBuilderExtensions).Assembly, "OneImlx.Terminal.Authentication");
        }

        [Fact]
        public void TypesLocationTest()
        {
            TestHelper.AssertAssemblyTypesLocation(typeof(Extensions.ITerminalBuilderExtensions).Assembly);
        }
    }
}