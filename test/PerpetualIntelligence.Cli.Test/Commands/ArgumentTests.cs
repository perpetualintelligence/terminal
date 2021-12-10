/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test.Services;
using System.ComponentModel.DataAnnotations;

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
        public void ArgumentsWithDifferentNameAreNotEqual()
        {
            Argument arg1 = new Argument("name1", "value1", DataType.Text);
            Argument arg2 = new Argument("name2", "value1", DataType.Text);

            Assert.AreNotEqual(arg1, arg2);
        }

        [TestMethod]
        public void ArgumentsWithSameNameAreEqual()
        {
            Argument arg1 = new Argument("name1", "value1", DataType.Text);
            Argument arg2 = new Argument("name1", 26.36, "Custom");

            Assert.AreEqual(arg1, arg2);
        }

        [TestMethod]
        public void JsonPropertyNamesShouldBeCorrect()
        {
            TestHelper.AssertJsonPropertyName(typeof(Argument).GetProperty(nameof(Argument.CustomDataType)), "custom_data_type");
            TestHelper.AssertJsonPropertyName(typeof(Argument).GetProperty(nameof(Argument.DataType)), "data_type");
            TestHelper.AssertJsonPropertyName(typeof(Argument).GetProperty(nameof(Argument.Description)), "description");
            TestHelper.AssertJsonPropertyName(typeof(Argument).GetProperty(nameof(Argument.Id)), "id");
            TestHelper.AssertJsonPropertyName(typeof(Argument).GetProperty(nameof(Argument.Value)), "value");
            TestHelper.AssertJsonPropertyName(typeof(Argument).GetProperty(nameof(Argument.Properties)), "properties");
        }
    }
}
