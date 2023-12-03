/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/
/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test.Services;
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
            TestHelper.AssertJsonPropertyName(typeof(Option).GetProperty(nameof(Option.Value)), "value");
            TestHelper.AssertJsonPropertyName(typeof(Option).GetProperty(nameof(Option.Descriptor)), "descriptor");

            typeof(Option).GetProperty(nameof(Option.Alias)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.DataType)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.DataType)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.Description)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.Id)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
        }
    }
}