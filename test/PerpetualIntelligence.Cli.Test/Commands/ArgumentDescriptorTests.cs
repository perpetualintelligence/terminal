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
            ArgumentDescriptor arg = new("name", "custom", required: true);
            Assert.AreEqual(DataType.Custom, arg.DataType);
        }

        [TestMethod]
        public void RequiredExplicitlySetShouldNotSetDataAnnotationRequiredAttribute()
        {
            ArgumentDescriptor arg = new("name", "custom", required: true);
            Assert.IsNull(arg.ValidationAttributes);
            Assert.IsTrue(arg.Required);
        }

        [TestMethod]
        public void RequiredShouldBeSetWithDataAnnotationRequiredAttribute()
        {
            ArgumentDescriptor arg = new("name", "custom", required: false) { ValidationAttributes = new[] { new RequiredAttribute() } };
            Assert.IsNotNull(arg.ValidationAttributes);
            CollectionAssert.Contains(arg.ValidationAttributes.ToArray(), new RequiredAttribute());
            Assert.IsTrue(arg.Required);
        }
    }
}
