/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Providers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Cli.Stores;
using PerpetualIntelligence.Cli.Stores.InMemory;
using PerpetualIntelligence.Shared.Attributes;
using PerpetualIntelligence.Test.Services;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    [TestClass]
    public class OptionsCheckerTests
    {
        public OptionsCheckerTests()
        {
            options = MockCliOptions.New();
            textHandler = new UnicodeTextHandler();

            hostBuilder = Host.CreateDefaultBuilder().ConfigureServices(services =>
            {
                services.AddSingleton<ITextHandler>(textHandler);
                services.AddSingleton(options);
            });
            host = hostBuilder.Start();

            optionsChecker = new OptionsChecker(host.Services);
            commands = new InMemoryCommandStore(textHandler, MockCommands.Commands, options, TestLogger.Create<InMemoryCommandStore>());
            argExtractor = new ArgumentExtractor(textHandler, options, TestLogger.Create<ArgumentExtractor>());
            defaultArgValueProvider = new DefaultArgumentValueProvider(textHandler);
            defaultArgProvider = new DefaultArgumentProvider(options, TestLogger.Create<DefaultArgumentProvider>());
        }

        [TestMethod]
        public async Task ArgumentAliasPrefixCannotBeNullOrWhitespaceAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Extractor.ArgumentAliasPrefix = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The argument alias prefix cannot be null or whitespace.");

            options.Extractor.ArgumentAliasPrefix = "";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The argument alias prefix cannot be null or whitespace.");

            options.Extractor.ArgumentAliasPrefix = "   ";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The argument alias prefix cannot be null or whitespace.");
        }

        [DataTestMethod]
        [DataRow("@")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणू")]
        [DataRow("女性")]
        public async Task ArgumentAliasPrefixCannotStartWithArgumentPrefix(string prefix)
        {
            options.Extractor.ArgumentPrefix = prefix;
            options.Extractor.ArgumentAliasPrefix = $"{prefix}:";

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The argument alias prefix cannot start with argument prefix. prefix={prefix}");
        }

        [DataTestMethod]
        [DataRow("@@@@")]
        [DataRow("~~~~")]
        [DataRow("####")]
        [DataRow("sp----")]
        [DataRow("öööö")]
        [DataRow("माणूसस")]
        [DataRow("女性女性")]
        [DataRow("-女माö")]
        public async Task ArgumentAliasPrefixWithMoreThan3UnicodeCharsShouldError(string prefix)
        {
            options.Extractor.ArgumentAliasPrefix = prefix;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The argument alias prefix cannot be more than 3 Unicode characters. argument_alias_prefix={prefix}");
        }

        [TestMethod]
        public async Task ArgumentPrefixCannotBeNullOrWhitespaceAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Extractor.ArgumentPrefix = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The argument prefix cannot be null or whitespace.");

            options.Extractor.ArgumentPrefix = "";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The argument prefix cannot be null or whitespace.");

            options.Extractor.ArgumentPrefix = "   ";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The argument prefix cannot be null or whitespace.");
        }

        [DataTestMethod]
        [DataRow("@@@@")]
        [DataRow("~~~~")]
        [DataRow("####")]
        [DataRow("sp----")]
        [DataRow("öööö")]
        [DataRow("माणूसस")]
        [DataRow("女性女性")]
        [DataRow("-女माö")]
        public async Task ArgumentPrefixWithMoreThan3UnicodeCharsShouldError(string prefix)
        {
            options.Extractor.ArgumentPrefix = prefix;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The argument prefix cannot be more than 3 Unicode characters. argument_prefix={prefix}");
        }

        [DataTestMethod]
        [DataRow("@")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task ArgumentSeparatorAndArgumentAliasPrefixCannotBeSameAsync(string separator)
        {
            options.Extractor.ArgumentValueSeparator = separator;
            options.Extractor.ArgumentPrefix = ":";
            options.Extractor.ArgumentAliasPrefix = separator;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The argument separator and argument alias prefix cannot be same. separator={separator}");
        }

        [DataTestMethod]
        [DataRow("@")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task ArgumentSeparatorAndArgumentPrefixCannotBeSameAsync(string separator)
        {
            options.Extractor.ArgumentValueSeparator = separator;
            options.Extractor.ArgumentPrefix = separator;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The argument separator and argument prefix cannot be same. separator={separator}");
        }

        [TestMethod]
        public async Task ArgumentSeparatorCannotBeNullOrEmptyAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Extractor.ArgumentValueSeparator = null;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, "The argument separator cannot be null or empty.");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            options.Extractor.ArgumentValueSeparator = "";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, "The argument separator cannot be null or empty.");
        }

        [DataTestMethod]
        [DataRow(" ")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandSeparatorAndArgumentPrefixCannotBeSameAsync(string separator)
        {
            options.Extractor.Separator = separator;
            options.Extractor.ArgumentPrefix = separator;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The command separator and argument prefix cannot be same. separator={separator}");
        }

        [TestMethod]
        public async Task CommandSeparatorCannotBeNullOrEmptyAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Extractor.Separator = null;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, "The command separator cannot be null or empty.");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            options.Extractor.Separator = "";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, "The command separator cannot be null or empty.");
        }

        [TestMethod]
        [WriteDocumentation]
        public async Task DefaultArgumentConfiguredButProviderNotConfiguredShouldThrow()
        {
            options.Extractor.DefaultArgument = true;

            CommandExtractorContext context = new(new CommandString("prefix6_defaultarg"));

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, "The command default argument provider is missing in the service collection. provider_type=PerpetualIntelligence.Cli.Commands.Providers.IDefaultArgumentProvider");
        }

        [TestMethod]
        public async Task DefaultValueConfiguredButProviderNotConfiguredShouldThrow()
        {
            options.Extractor.DefaultArgumentValue = true;

            CommandExtractorContext context = new(new CommandString("prefix5_default"));

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, "The argument default value provider is missing in the service collection. provider_type=PerpetualIntelligence.Cli.Commands.Providers.IDefaultArgumentValueProvider");
        }

        [TestMethod]
        public async Task WithInStringCannotBeSameAsArgAliasPrefix()
        {
            // Make sure command separator is different so we can fail for argument separator below.
            options.Extractor.ArgumentPrefix = "#";
            options.Extractor.ArgumentValueWithIn = "^";
            options.Extractor.ArgumentAliasPrefix = "^";

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The string with_in token and argument alias prefix cannot be same. with_in=^");
        }

        [TestMethod]
        public async Task WithInStringCannotBeSameAsArgPrefix()
        {
            // Make sure command separator is different so we can fail for argument separator below.
            options.Extractor.ArgumentPrefix = "^";
            options.Extractor.ArgumentValueWithIn = "^";

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The string with_in token and argument prefix cannot be same. with_in=^");
        }

        [TestMethod]
        public async Task WithInStringCannotBeSameAsArgSeparator()
        {
            // Make sure command separator is different so we can fail for argument separator below.
            options.Extractor.ArgumentValueSeparator = "^";
            options.Extractor.ArgumentValueWithIn = "^";

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The string with_in token and argument separator cannot be same. with_in=^");
        }

        [TestMethod]
        public async Task WithInStringCannotBeSameAsSeparator()
        {
            // Make sure command separator is different so we can fail for argument separator below.
            options.Extractor.Separator = "^";
            options.Extractor.ArgumentValueWithIn = "^";

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The string with_in token and separator cannot be same. with_in=^");
        }

        [TestMethod]
        public async Task WithInStringCannotBeWhitespace()
        {
            // Make sure command separator is different so we can fail for argument separator below.
            options.Extractor.ArgumentValueWithIn = "   ";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The string with_in token cannot be whitespace.");
        }

        private ArgumentExtractor argExtractor;
        private ICommandStoreHandler commands;
        private IDefaultArgumentProvider defaultArgProvider = null!;
        private IDefaultArgumentValueProvider defaultArgValueProvider = null!;
        private IHost host;
        private IHostBuilder hostBuilder;
        private CliOptions options;
        private IOptionsChecker optionsChecker;
        private ITextHandler textHandler;
    }
}
