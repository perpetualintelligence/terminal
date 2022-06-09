﻿/*
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
        public void RequiredShouldSetRequiredDataAnnotatioNAttribute()
        {
            ArgumentDescriptor arg = new("name", "custom", required: true);
            Assert.IsNotNull(arg.ValidationAttributes);
            CollectionAssert.Contains(arg.ValidationAttributes.ToArray(), new RequiredAttribute());
            Assert.IsTrue(arg.Required);

            ArgumentDescriptor arg2 = new("name", DataType.CreditCard, required: true);
            Assert.IsNotNull(arg2.ValidationAttributes);
            CollectionAssert.Contains(arg2.ValidationAttributes.ToArray(), new RequiredAttribute());
            Assert.IsTrue(arg2.Required);
        }
    }
}
