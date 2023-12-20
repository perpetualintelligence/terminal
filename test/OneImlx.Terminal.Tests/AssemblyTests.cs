/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal
{
    public class AssemblyTests
    {
        [Fact]
        public void TypesNamespaceTest()
        {
            typeof(TerminalErrors).Assembly.Should().HaveTypesInRootNamespace("OneImlx.Terminal");
        }

        [Fact]
        public void TypesLocationTest()
        {
            typeof(TerminalErrors).Assembly.Should().HaveTypesInValidLocations();
        }
    }
}