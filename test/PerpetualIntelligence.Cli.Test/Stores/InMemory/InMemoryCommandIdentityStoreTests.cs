/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Stores.InMemory
{
    [TestClass]
    public class InMemoryCommandIdentityStoreTests : LogTest
    {
        public InMemoryCommandIdentityStoreTests() : base(TestLogger.Create<InMemoryCommandIdentityStoreTests>())
        {
        }

        [TestMethod]
        public async Task TryFindByIdShouldErrorIfNotFoundAsync()
        {
            var result = await cmdStore.TryFindByIdAsync("invalid_id");
            TestHelper.AssertOneImlxError(result, Errors.UnsupportedCommand, "The command id is not valid. id=invalid_id");
        }

        [TestMethod]
        public async Task TryFindByIdShouldNotErrorIfFoundAsync()
        {
            var result = await cmdStore.TryFindByIdAsync("id1");
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("id1", result.Result.Id);
            Assert.AreEqual("name1", result.Result.Name);
            Assert.AreEqual("prefix1", result.Result.Prefix);
        }

        [TestMethod]
        public async Task TryFindByNameShouldErrorIfNotFoundAsync()
        {
            var result = await cmdStore.TryFindByNameAsync("invalid_name");
            TestHelper.AssertOneImlxError(result, Errors.UnsupportedCommand, "The command name is not valid. name=invalid_name");
        }

        [TestMethod]
        public async Task TryFindByNameShouldNotErrorIfFoundAsync()
        {
            var result = await cmdStore.TryFindByNameAsync("name1");
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("id1", result.Result.Id);
            Assert.AreEqual("name1", result.Result.Name);
            Assert.AreEqual("prefix1", result.Result.Prefix);
        }

        [TestMethod]
        public async Task TryFindByPrefixShouldErrorIfNotFoundAsync()
        {
            var result = await cmdStore.TryFindByPrefixAsync("invalid_prefix");
            TestHelper.AssertOneImlxError(result, Errors.UnsupportedCommand, "The command prefix is not valid. prefix=invalid_prefix");
        }

        [TestMethod]
        public async Task TryFindByPrefixShouldNotErrorIfFoundAsync()
        {
            var result = await cmdStore.TryFindByPrefixAsync("prefix1");
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("id1", result.Result.Id);
            Assert.AreEqual("name1", result.Result.Name);
            Assert.AreEqual("prefix1", result.Result.Prefix);
        }

        protected override void OnTestInitialize()
        {
            cmds = MockCommands.Commands;
            options = MockCliOptions.New();
            cmdStore = new InMemoryCommandIdentityStore(cmds, options, TestLogger.Create<InMemoryCommandIdentityStore>());
        }

        private IEnumerable<CommandDescriptor> cmds = null!;
        private InMemoryCommandIdentityStore cmdStore = null!;
        private CliOptions options = null!;
    }
}
