/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Authentication
{
    public class AssemblyTests
    {
        [Fact]
        public void TypesNamespaceTest()
        {
            typeof(Extensions.ITerminalBuilderExtensions).Assembly.Should().HaveTypesInRootNamespace("OneImlx.Terminal.Authentication");
        }

        [Fact]
        public void TypesLocationTest()
        {
            typeof(Extensions.ITerminalBuilderExtensions).Assembly.Should().HaveTypesInValidLocations();
        }
    }
}