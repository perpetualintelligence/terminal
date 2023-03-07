/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PerpetualIntelligence.Cli.Commands
{
    [TestClass]
    public class ArgumentDescriptorTests
    {
        [TestMethod]
        public void CustomDataTypeShouldSetDataType()
        {
            ArgumentDescriptor arg = new("name", "custom", "test desc", required: true);
            Assert.AreEqual(DataType.Custom, arg.DataType);
        }

        [TestMethod]
        public void RequiredExplicitlySetShouldNotSetDataAnnotationRequiredAttribute()
        {
            ArgumentDescriptor arg = new("name", "custom", "test desc", required: true);
            Assert.IsNull(arg.ValueCheckers);
            Assert.IsTrue(arg.Required);
        }

        [TestMethod]
        public void RequiredShouldBeSetWithDataAnnotationRequiredAttribute()
        {
            ArgumentDescriptor arg = new("name", "custom", "test desc", required: false) { ValueCheckers = new[] { new RequiredAttribute() } };
            Assert.IsNotNull(arg.ValueCheckers);
            CollectionAssert.Contains(arg.ValueCheckers.ToArray(), new RequiredAttribute());
            Assert.IsTrue(arg.Required);
        }
    }
}
