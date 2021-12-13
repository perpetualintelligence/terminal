/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PerpetualIntelligence.Cli
{
    [TestClass]
    public class AssemblyAttributesTests
    {
        [TestMethod]
        public void InternalsVisibleToShouldSetCorrectly()
        {
            var internalAttrs = typeof(Commands.Command).Assembly.GetCustomAttributes<InternalsVisibleToAttribute>();
            Assert.AreEqual(1, internalAttrs.Count());
            Assert.AreEqual("PerpetualIntelligence.Cli.Test", internalAttrs.First().AssemblyName);
        }
    }
}
