/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Commands
{
    public class ArgumentTests
    {
        [Fact]
        public void ArgumentShouldBeSealed()
        {
            typeof(Option).IsSealed.Should().BeTrue();
        }

        [Fact]
        public void OptionsWithDifferentIdAreNotEqual()
        {
            Option opt1 = new(new OptionDescriptor("id1", nameof(String), "desc1", OptionFlags.None), "value1");
            Option opt2 = new(new OptionDescriptor("id2", nameof(String), "desc1", OptionFlags.None), "value1");

            opt1.Should().NotBe(opt2);
        }

        [Fact]
        public void OptionsWithSameIdAreEqual()
        {
            Option opt1 = new(new OptionDescriptor("id1", nameof(String), "desc1", OptionFlags.None), "value1");
            Option opt2 = new(new OptionDescriptor("id1", "Custom", "desc1", OptionFlags.None), 25.64);

            opt1.Should().Be(opt2);
        }
    }
}
