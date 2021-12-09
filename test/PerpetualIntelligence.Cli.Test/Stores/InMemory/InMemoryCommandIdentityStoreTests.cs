/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
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
    public class InMemoryCommandIdentityStoreTests : OneImlxLogTest
    {
        public InMemoryCommandIdentityStoreTests() : base(TestLogger.Create<InMemoryCommandIdentityStoreTests>())
        {
        }

        [TestMethod]
        public async Task TryFindByIdShouldErrorIfNotFoundAsync()
        {
            var result = await cmdStore.TryFindByIdAsync("invalid_id");
            TestHelper.AssertOneImlxError(result, Errors.InvalidCommand, "The command id is not valid. id=invalid_id");
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
            TestHelper.AssertOneImlxError(result, Errors.InvalidCommand, "The command name is not valid. name=invalid_name");
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
            TestHelper.AssertOneImlxError(result, Errors.InvalidCommand, "The command prefix is not valid. prefix=invalid_prefix");
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

        [TestMethod]
        public async Task TryMatchByPrefixShouldErrorIfNotFoundAsync()
        {
            var result = await cmdStore.TryMatchByPrefixAsync("invalid prefix1");
            TestHelper.AssertOneImlxError(result, Errors.InvalidCommand, "The command string did not match any command prefix. command_string=invalid prefix1");
        }

        [TestMethod]
        public async Task TryMatchByPrefixShouldNotErrorIfMatchedAsync()
        {
            var result = await cmdStore.TryMatchByPrefixAsync("prefix1");
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("id1", result.Result.Id);
            Assert.AreEqual("name1", result.Result.Name);
            Assert.AreEqual("prefix1", result.Result.Prefix);
        }

        [TestMethod]
        public async Task TryMatchByPrefixWithValidArgsShouldNotErrorIfMatchedAsync()
        {
            var result = await cmdStore.TryMatchByPrefixAsync("prefix1 -key1=value1 -key2=value2");
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("id1", result.Result.Id);
            Assert.AreEqual("name1", result.Result.Name);
            Assert.AreEqual("prefix1", result.Result.Prefix);
        }

        [TestMethod]
        public async Task TryMatchByPrefixWithInValidArgsShouldNotErrorIfMatchedAsync()
        {
            var result = await cmdStore.TryMatchByPrefixAsync("prefix1 -invalidarg=test -invalidsolo -key1=value1 -key2=value2");
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("id1", result.Result.Id);
            Assert.AreEqual("name1", result.Result.Name);
            Assert.AreEqual("prefix1", result.Result.Prefix);
        }

        [TestMethod]
        public async Task TryMatchByPrefixWithInsufficientPharasesShouldErrorAsync()
        {
            // Missing name3
            var result = await cmdStore.TryMatchByPrefixAsync("prefix3 sub3");
            TestHelper.AssertOneImlxError(result, Errors.InvalidCommand, "The command string did not match any command prefix. command_string=prefix3 sub3");
        }

        [TestMethod]
        public async Task TryMatchByPrefixWithMultiplePharasesShouldNotErrorIfMatchedAsync()
        {
            var result = await cmdStore.TryMatchByPrefixAsync("prefix3 sub3 name3");
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("id3", result.Result.Id);
            Assert.AreEqual("name3", result.Result.Name);
            Assert.AreEqual("prefix3 sub3 name3", result.Result.Prefix);
        }

        protected override void OnTestInitialize()
        {
            cmds = MockCommands.Commands;
            options = MockCliOptions.New();
            cmdStore = new InMemoryCommandIdentityStore(cmds, options, TestLogger.Create<InMemoryCommandIdentityStore>());
        }

        private IEnumerable<CommandIdentity> cmds = null!;
        private InMemoryCommandIdentityStore cmdStore = null!;
        private CliOptions options = null!;
    }
}
