/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    [TestClass]
    public class ArgumentDefaultValueProviderTests : LogTestWithLogger<ArgumentDefaultValueProviderTests>
    {
        [TestMethod]
        public async Task NullOrEmptyArgumentDescriptorsShouldThrowAsync()
        {
            ArgumentDefaultValueProvider provider = new(MockCliOptions.New(), TestLogger.Create<ArgumentDefaultValueProvider>());

            ArgumentDefaultValueProviderContext nullContext = new(new CommandDescriptor("test", "testname", "testprefix", argumentDescriptors: null));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => provider.ProvideAsync(nullContext), Errors.UnsupportedArgument, "The command does not support any arguments. command_id=test command_name=testname");

            ArgumentDefaultValueProviderContext emptyContext = new(new CommandDescriptor("test_empty", "test_emptyname", "test_emptyprefix", argumentDescriptors: new()));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => provider.ProvideAsync(emptyContext), Errors.UnsupportedArgument, "The command does not support any arguments. command_id=test_empty command_name=test_emptyname");
        }

        [TestMethod]
        public async Task ProviderShouldProvideDefaultArgumentsCorrectly()
        {
            ArgumentDefaultValueProvider provider = new(MockCliOptions.New(), TestLogger.Create<ArgumentDefaultValueProvider>());
            ArgumentDefaultValueProviderContext context = new(new CommandDescriptor("test", "testname", "testprefix", MockCommands.TestDefaultArgumentDescriptors));
            var result = await provider.ProvideAsync(context);
            Assert.AreEqual(4, result.DefaultValueArgumentDescriptors.Count);
            Assert.AreEqual("44444444444", result.DefaultValueArgumentDescriptors[0].DefaultValue);
            Assert.AreEqual(false, result.DefaultValueArgumentDescriptors[1].DefaultValue);
            Assert.AreEqual(25.36, result.DefaultValueArgumentDescriptors[2].DefaultValue);
            Assert.AreEqual("mello default", result.DefaultValueArgumentDescriptors[3].DefaultValue);
        }
    }
}
