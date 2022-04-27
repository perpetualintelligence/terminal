/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Cli
{
    [TestClass]
    public class HandlersTests
    {
        [TestMethod]
        public void AssertHandlersAreValid()
        {
            TestHelper.AssertConstantCount(typeof(Handlers), 7);

            Assert.AreEqual("boyl", Handlers.BoylHandler);
            Assert.AreEqual("custom", Handlers.CustomHandler);
            Assert.AreEqual("default", Handlers.DefaultHandler);
            Assert.AreEqual("in-memory", Handlers.InMemoryHandler);
            Assert.AreEqual("json", Handlers.JsonHandler);
            Assert.AreEqual("offline", Handlers.OfflineHandler);
            Assert.AreEqual("online", Handlers.OnlineHandler);
        }
    }
}
