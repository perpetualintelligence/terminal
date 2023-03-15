/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PerpetualIntelligence.Cli.Commands
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
            Option arg1 = new(new OptionDescriptor("id1", DataType.Text, "desc1"), "value1");
            Option arg2 = new(new OptionDescriptor("id2", DataType.Text, "desc1"), "value1");

            Assert.AreNotEqual(arg1, arg2);
        }

        [TestMethod]
        public void OptionsWithSameIdAreEqual()
        {
            Option arg1 = new(new OptionDescriptor("id1", DataType.Text, "desc1"), "value1");
            Option arg2 = new(new OptionDescriptor("id1", "Custom", "desc1"), 25.64);

            Assert.AreEqual(arg1, arg2);
        }

        [TestMethod]
        public void JsonPropertyNamesShouldBeCorrect()
        {
            TestHelper.AssertJsonPropertyName(typeof(Option).GetProperty(nameof(Option.Value)), "value");
            TestHelper.AssertJsonPropertyName(typeof(Option).GetProperty(nameof(Option.Descriptor)), "descriptor");

            typeof(Option).GetProperty(nameof(Option.Alias)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.CustomDataType)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.CustomProperties)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.DataType)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.Description)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Option).GetProperty(nameof(Option.Id)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
        }
    }
}