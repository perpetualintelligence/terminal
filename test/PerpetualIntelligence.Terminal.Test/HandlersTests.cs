/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Test.Services;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace PerpetualIntelligence.Terminal
{
    public class HandlersTests
    {
        [Fact]
        public void AssertHandlersAreValid()
        {
            TestHelper.AssertConstantCount(typeof(Handlers), 9);

            Handlers.DevLicenseHandler.Should().Be("dev-license");
            Handlers.CustomHandler.Should().Be("custom");
            Handlers.DefaultHandler.Should().Be("default");
            Handlers.InMemoryHandler.Should().Be("in-memory");
            Handlers.JsonHandler.Should().Be("json");
            Handlers.OfflineLicenseHandler.Should().Be("offline-license");
            Handlers.OnlineLicenseHandler.Should().Be("online-license");
            Handlers.UnicodeHandler.Should().Be("unicode");
            Handlers.AsciiHandler.Should().Be("ascii");
        }

        [Fact]
        public void Handler_Defines_InternalVisibleTo_ForUnitTests()
        {
            InternalsVisibleToAttribute? internalsVisibleToAttribute = typeof(Handlers).Assembly.GetCustomAttribute<InternalsVisibleToAttribute>();
            internalsVisibleToAttribute.Should().NotBeNull();
            internalsVisibleToAttribute!.AssemblyName.Should().Be("PerpetualIntelligence.Terminal.Test");
        }
    }
}