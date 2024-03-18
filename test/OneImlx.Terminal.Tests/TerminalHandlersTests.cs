/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Test.FluentAssertions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace OneImlx.Terminal
{
    public class TerminalHandlersTests
    {
        [Fact]
        public void AssertHandlersAreValid()
        {
            typeof(TerminalHandlers).Should().HaveConstantCount(2);

            TerminalHandlers.CustomHandler.Should().Be("custom");
            TerminalHandlers.DefaultHandler.Should().Be("default");
        }

        [Fact]
        public void Handler_Defines_InternalVisibleTo_ForUnitTests()
        {
            InternalsVisibleToAttribute? internalsVisibleToAttribute = typeof(TerminalHandlers).Assembly.GetCustomAttribute<InternalsVisibleToAttribute>();
            internalsVisibleToAttribute.Should().NotBeNull();
            internalsVisibleToAttribute!.AssemblyName.Should().Be("OneImlx.Terminal.Tests");
        }
    }
}