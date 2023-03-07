/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Checkers;
using System.ComponentModel.DataAnnotations;

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
            ArgumentDescriptor arg = new("name", "custom", "test desc", required: false) { ValueCheckers = new[] { new DataValidationArgumentValueChecker(new RequiredAttribute()) } };
            Assert.IsNotNull(arg.ValueCheckers);
            Assert.IsTrue(arg.Required);
        }
    }
}