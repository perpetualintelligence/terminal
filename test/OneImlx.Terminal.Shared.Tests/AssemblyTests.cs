/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Shared
{
    public class AssemblyTests
    {
        [Fact]
        public void TypesLocationTest()
        {
            typeof(TerminalErrors).Assembly.Should().HaveTypesInValidLocations();
        }

        [Fact]
        public void TypesNamespaceTest()
        {
            typeof(TerminalErrors).Assembly.Should().HaveTypesInRootNamespace("OneImlx.Terminal.Shared");
        }
    }
}
