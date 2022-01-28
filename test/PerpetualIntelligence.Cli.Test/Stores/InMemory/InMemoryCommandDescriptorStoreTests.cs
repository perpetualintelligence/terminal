/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Stores.InMemory
{
    [TestClass]
    public class InMemoryCommandDescriptorStoreTests : LogTest
    {
        public InMemoryCommandDescriptorStoreTests() : base(TestLogger.Create<InMemoryCommandDescriptorStoreTests>())
        {
        }

        [TestMethod]
        public async Task TryFindByIdShouldErrorIfNotFoundAsync()
        {
            var result = await cmdStore.TryFindByIdAsync("invalid_id");
            TestHelper.AssertTryResultError(result, Errors.UnsupportedCommand, "The command id is not valid. id=invalid_id");
        }

        [TestMethod]
        public async Task TryFindByIdShouldNotErrorIfFoundAsync()
        {
            var result = await cmdStore.TryFindByIdAsync("id1");
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("id1", result.Result.Id);
            Assert.AreEqual("name1", result.Result.Name);
            Assert.AreEqual("prefix1", result.Result.Prefix);
        }

        [TestMethod]
        public async Task TryFindByNameShouldErrorIfNotFoundAsync()
        {
            var result = await cmdStore.TryFindByNameAsync("invalid_name");
            TestHelper.AssertTryResultError(result, Errors.UnsupportedCommand, "The command name is not valid. name=invalid_name");
        }

        [TestMethod]
        public async Task TryFindByNameShouldNotErrorIfFoundAsync()
        {
            var result = await cmdStore.TryFindByNameAsync("name1");
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("id1", result.Result.Id);
            Assert.AreEqual("name1", result.Result.Name);
            Assert.AreEqual("prefix1", result.Result.Prefix);
        }

        [TestMethod]
        public async Task TryFindByPrefixShouldErrorIfNotFoundAsync()
        {
            var result = await cmdStore.TryFindByPrefixAsync("invalid_prefix");
            TestHelper.AssertTryResultError(result, Errors.UnsupportedCommand, "The command prefix is not valid. prefix=invalid_prefix");
        }

        [TestMethod]
        public async Task TryFindByPrefixShouldNotErrorIfFoundAsync()
        {
            var result = await cmdStore.TryFindByPrefixAsync("prefix1");
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("id1", result.Result.Id);
            Assert.AreEqual("name1", result.Result.Name);
            Assert.AreEqual("prefix1", result.Result.Prefix);
        }

        protected override void OnTestInitialize()
        {
            cmds = MockCommands.Commands;
            cmdStore = new InMemoryCommandDescriptorStore(cmds);
        }

        private IEnumerable<CommandDescriptor> cmds = null!;
        private InMemoryCommandDescriptorStore cmdStore = null!;
    }
}
