/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.Json.Serialization;

namespace OneImlx.Terminal.Commands
{
    [TestClass]
    public class ArgumentTests
    {
        [TestMethod]
        public void ArgumentShouldBeSealed()
        {
            Assert.IsTrue(typeof(Option).IsSealed);
        }

        [TestMethod]
        public void OptionsWithDifferentIdAreNotEqual()
        {
            Option opt1 = new(new OptionDescriptor("id1", nameof(String), "desc1", OptionFlags.None), "value1");
            Option opt2 = new(new OptionDescriptor("id2", nameof(String), "desc1", OptionFlags.None), "value1");

            Assert.AreNotEqual(opt1, opt2);
        }

        [TestMethod]
        public void OptionsWithSameIdAreEqual()
        {
            Option opt1 = new(new OptionDescriptor("id1", nameof(String), "desc1", OptionFlags.None), "value1");
            Option opt2 = new(new OptionDescriptor("id1", "Custom", "desc1", OptionFlags.None), 25.64);

            Assert.AreEqual(opt1, opt2);
        }

        [TestMethod]
        public void JSONPropertyNamesShouldBeCorrect()
        {
            typeof(Option).GetProperty(nameof(Option.Value)).Should().BeDecoratedWith<JsonPropertyNameAttribute>(attr => attr.Name == "value");
            typeof(Option).GetProperty(nameof(Option.Descriptor)).Should().BeDecoratedWith<JsonPropertyNameAttribute>(attr => attr.Name == "descriptor");

            typeof(Option).GetProperty(nameof(Option.Alias)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.DataType)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.DataType)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.Description)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.Id)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
        }
    }
}