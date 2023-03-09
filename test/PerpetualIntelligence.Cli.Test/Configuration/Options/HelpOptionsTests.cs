﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using Xunit;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    public class HelpOptionsTests : InitializerTests
    {
        public HelpOptionsTests() : base(TestLogger.Create<HelpOptionsTests>())
        {
        }

        [Fact]
        public void ShouldHaveCorrectDefaultValues()
        {
            HelpOptions options = new();

            options.HelpArgumentAlias.Should().Be("H");
            options.HelpArgumentId.Should().Be("help");
            options.Disabled.Should().BeNull();
        }
    }
}