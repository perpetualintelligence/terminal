/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Stores.InMemory
{
    [TestClass]
    public class InMemoryCommandDescriptorStoreTests : InitializerTests
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

        [TestMethod]
        public async Task TryMatchByPrefixGroupAndNestedOAuthWithDefaultArgValueShouldMatchExact()
        {
            var result = await groupedCmdStore.TryMatchByPrefixAsync("pi auth slogin defaultargvalue1");
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("orgid:authid:sloginid", result.Result.Id);
            Assert.AreEqual("slogin", result.Result.Name);
            Assert.AreEqual("pi auth slogin", result.Result.Prefix); // Exact match

            result = await groupedCmdStore.TryMatchByPrefixAsync("pi auth slogin oauth defaultargvalue2");
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("orgid:authid:sloginid:oauth", result.Result.Id);
            Assert.AreEqual("oauth", result.Result.Name);
            Assert.AreEqual("pi auth slogin oauth", result.Result.Prefix); // Exact match
        }

        [TestMethod]
        public async Task TryMatchByPrefixInvalidPrefixShouldError()
        {
            var result = await groupedCmdStore.TryMatchByPrefixAsync("pi_invalid auth slogin");
            TestHelper.AssertTryResultError(result, Errors.UnsupportedCommand, "The command prefix is not valid. prefix=pi_invalid auth slogin");
        }

        [TestMethod]
        public async Task TryMatchByPrefixInvalidSubCommandPrefixShouldError()
        {
            var result = await groupedCmdStore.TryMatchByPrefixAsync("pi auth loginid invalid_command");
            TestHelper.AssertTryResultError(result, Errors.UnsupportedCommand, "The command prefix is not valid. prefix=pi auth loginid invalid_command");
        }

        [TestMethod]
        public async Task TryMatchByPrefixNestedOAuthShouldMatchExact()
        {
            var result = await groupedCmdStore.TryMatchByPrefixAsync("pi auth slogin oauth");
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("orgid:authid:sloginid:oauth", result.Result.Id);
            Assert.AreEqual("oauth", result.Result.Name);
            Assert.AreEqual("pi auth slogin oauth", result.Result.Prefix); // Exact match
        }

        [TestMethod]
        public async Task TryMatchByPrefixNestedOidcShouldMatchExact()
        {
            var result = await groupedCmdStore.TryMatchByPrefixAsync("pi auth slogin oidc");
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("orgid:authid:sloginid:oidc", result.Result.Id);
            Assert.AreEqual("oidc", result.Result.Name);
            Assert.AreEqual("pi auth slogin oidc", result.Result.Prefix); // Exact match
        }

        [TestMethod]
        public async Task TryMatchByPrefixShouldMatchExact()
        {
            var result = await groupedCmdStore.TryMatchByPrefixAsync("pi auth slogin");
            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("orgid:authid:sloginid", result.Result.Id);
            Assert.AreEqual("slogin", result.Result.Name);
            Assert.AreEqual("pi auth slogin", result.Result.Prefix); // Exact match
        }

        [TestMethod]
        public async Task TryMatchByPrefixValidTopGroupInvalidSubGroupValidShouldError()
        {
            var result = await groupedCmdStore.TryMatchByPrefixAsync("pi invalid_auth slogin");
            TestHelper.AssertTryResultError(result, Errors.UnsupportedCommand, "The command prefix is not valid. prefix=pi invalid_auth slogin");
        }

        protected override void OnTestInitialize()
        {
            options = MockTerminalOptions.New();
            textHandler = new UnicodeTextHandler();

            cmds = MockCommands.Commands;
            cmdStore = new InMemoryCommandStore(textHandler, cmds, options, TestLogger.Create<InMemoryCommandStore>());

            groupedCmds = MockCommands.GroupedCommands;
            groupedCmdStore = new InMemoryCommandStore(textHandler, groupedCmds, options, TestLogger.Create<InMemoryCommandStore>());
        }

        private IEnumerable<CommandDescriptor> cmds = null!;
        private InMemoryCommandStore cmdStore = null!;
        private IEnumerable<CommandDescriptor> groupedCmds = null!;
        private InMemoryCommandStore groupedCmdStore = null!;
        private TerminalOptions options = null!;
        private ITextHandler textHandler = null!;
    }
}
