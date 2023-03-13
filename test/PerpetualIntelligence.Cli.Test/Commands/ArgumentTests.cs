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
            Assert.IsTrue(typeof(Argument).IsSealed);
        }

        [TestMethod]
        public void ArgumentsWithDifferentIdAreNotEqual()
        {
            Argument arg1 = new(new ArgumentDescriptor("id1", DataType.Text, "desc1"), "value1");
            Argument arg2 = new(new ArgumentDescriptor("id2", DataType.Text, "desc1"), "value1");

            Assert.AreNotEqual(arg1, arg2);
        }

        [TestMethod]
        public void ArgumentsWithSameIdAreEqual()
        {
            Argument arg1 = new(new ArgumentDescriptor("id1", DataType.Text, "desc1"), "value1");
            Argument arg2 = new(new ArgumentDescriptor("id1", "Custom", "desc1"), 25.64);

            Assert.AreEqual(arg1, arg2);
        }

        [TestMethod]
        public void JsonPropertyNamesShouldBeCorrect()
        {
            TestHelper.AssertJsonPropertyName(typeof(Argument).GetProperty(nameof(Argument.Value)), "value");
            TestHelper.AssertJsonPropertyName(typeof(Argument).GetProperty(nameof(Argument.Descriptor)), "descriptor");

            typeof(Argument).GetProperty(nameof(Argument.Alias)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Argument).GetProperty(nameof(Argument.CustomDataType)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Argument).GetProperty(nameof(Argument.CustomProperties)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Argument).GetProperty(nameof(Argument.DataType)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Argument).GetProperty(nameof(Argument.Description)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
            typeof(Argument).GetProperty(nameof(Argument.Id)).Should().BeDecoratedWith<JsonIgnoreAttribute>();
        }
    }
}