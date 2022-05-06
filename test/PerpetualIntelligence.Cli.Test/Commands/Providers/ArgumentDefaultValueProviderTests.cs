/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    [TestClass]
    public class ArgumentDefaultValueProviderTests : LoggerTests<ArgumentDefaultValueProviderTests>
    {
        [TestMethod]
        public async Task NullOrEmptyArgumentDescriptorsShouldThrowAsync()
        {
            DefaultArgumentValueProvider provider = new(new UnicodeTextHandler());

            DefaultArgumentValueProviderContext nullContext = new(new CommandDescriptor("test", "testname", "testprefix", "desc", argumentDescriptors: null));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => provider.ProvideAsync(nullContext), Errors.UnsupportedArgument, "The command does not support any arguments. command_id=test command_name=testname");

            DefaultArgumentValueProviderContext emptyContext = new(new CommandDescriptor("test_empty", "test_emptyname", "test_emptyprefix", "desc", argumentDescriptors: new(new UnicodeTextHandler())));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => provider.ProvideAsync(emptyContext), Errors.UnsupportedArgument, "The command does not support any arguments. command_id=test_empty command_name=test_emptyname");
        }

        [TestMethod]
        public async Task ProviderShouldProvideDefaultArgumentsCorrectly()
        {
            DefaultArgumentValueProvider provider = new(new UnicodeTextHandler());
            DefaultArgumentValueProviderContext context = new(new CommandDescriptor("test", "testname", "testprefix", "desc", argumentDescriptors: MockCommands.TestDefaultArgumentDescriptors));
            var result = await provider.ProvideAsync(context);
            Assert.AreEqual(4, result.DefaultValueArgumentDescriptors.Count);
            Assert.AreEqual("44444444444", result.DefaultValueArgumentDescriptors[0].DefaultValue);
            Assert.AreEqual(false, result.DefaultValueArgumentDescriptors[1].DefaultValue);
            Assert.AreEqual(25.36, result.DefaultValueArgumentDescriptors[2].DefaultValue);
            Assert.AreEqual("mello default", result.DefaultValueArgumentDescriptors[3].DefaultValue);
        }
    }
}
