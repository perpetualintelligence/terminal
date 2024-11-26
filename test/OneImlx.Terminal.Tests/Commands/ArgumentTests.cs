/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using System;
using System.Text.Json.Serialization;
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

        [Fact]
        public void JSONPropertyNamesShouldBeCorrect()
        {
            typeof(Option).GetProperty(nameof(Option.Value)).Should().BeDecoratedWith<JsonPropertyNameAttribute>(static attr => attr.Name == "value");
            typeof(Option).GetProperty(nameof(Option.Descriptor)).Should().BeDecoratedWith<JsonPropertyNameAttribute>(static attr => attr.Name == "descriptor");

            typeof(Option).GetProperty(nameof(Option.Alias)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.DataType)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.DataType)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.Description)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.Id)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
        }
    }
}