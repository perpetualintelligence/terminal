/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Test.Services;
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
            TestHelper.AssertConstantCount(typeof(TerminalHandlers), 9);

            
            TerminalHandlers.CustomHandler.Should().Be("custom");
            TerminalHandlers.DefaultHandler.Should().Be("default");
            TerminalHandlers.InMemoryHandler.Should().Be("in-memory");
            TerminalHandlers.JsonHandler.Should().Be("json");
            TerminalHandlers.OfflineLicenseHandler.Should().Be("offline-license");
            TerminalHandlers.OnlineLicenseHandler.Should().Be("online-license");
            TerminalHandlers.OnPremiseLicenseHandler.Should().Be("onpremise-license");
            TerminalHandlers.UnicodeHandler.Should().Be("unicode");
            TerminalHandlers.AsciiHandler.Should().Be("ascii");
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