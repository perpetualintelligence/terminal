/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Test.Services;
using Xunit;

namespace PerpetualIntelligence.Cli
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
    }
}