/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Commands.Extractors;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Providers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Stores;
using PerpetualIntelligence.Terminal.Stores.InMemory;
using PerpetualIntelligence.Shared.Attributes;
using PerpetualIntelligence.Test.Services;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
{
    [TestClass]
    public class ConfigurationOptionsCheckerTests
    {
        public ConfigurationOptionsCheckerTests()
        {
            options = MockTerminalOptions.New();
            textHandler = new UnicodeTextHandler();

            hostBuilder = Host.CreateDefaultBuilder().ConfigureServices(services =>
            {
                services.AddSingleton(textHandler);
                services.AddSingleton(options);
            });
            host = hostBuilder.Start();

            optionsChecker = new ConfigurationOptionsChecker(host.Services);
            commands = new InMemoryCommandStore(textHandler, MockCommands.Commands, options, TestLogger.Create<InMemoryCommandStore>());
            argExtractor = new OptionExtractor(textHandler, options, TestLogger.Create<OptionExtractor>());
            defaultArgValueProvider = new DefaultOptionValueProvider(textHandler);
            defaultArgProvider = new DefaultOptionProvider(options, TestLogger.Create<DefaultOptionProvider>());
        }

        [TestMethod]
        public async Task OptionAliasPrefixCannotBeNullOrWhitespaceAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Extractor.OptionAliasPrefix = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The option alias prefix cannot be null or whitespace.");

            options.Extractor.OptionAliasPrefix = "";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The option alias prefix cannot be null or whitespace.");

            options.Extractor.OptionAliasPrefix = "   ";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The option alias prefix cannot be null or whitespace.");
        }

        [DataTestMethod]
        [DataRow("@")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणू")]
        [DataRow("女性")]
        public async Task OptionAliasPrefixCannotStartWithOptionPrefix(string prefix)
        {
            options.Extractor.OptionPrefix = prefix;
            options.Extractor.OptionAliasPrefix = $"{prefix}:";

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The option alias prefix cannot start with option prefix. prefix={prefix}");
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
        public async Task OptionAliasPrefixWithMoreThan3UnicodeCharsShouldError(string prefix)
        {
            options.Extractor.OptionAliasPrefix = prefix;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The option alias prefix cannot be more than 3 Unicode characters. option_alias_prefix={prefix}");
        }

        [TestMethod]
        public async Task OptionPrefixCannotBeNullOrWhitespaceAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Extractor.OptionPrefix = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The option prefix cannot be null or whitespace.");

            options.Extractor.OptionPrefix = "";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The option prefix cannot be null or whitespace.");

            options.Extractor.OptionPrefix = "   ";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The option prefix cannot be null or whitespace.");
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
        public async Task OptionPrefixWithMoreThan3UnicodeCharsShouldError(string prefix)
        {
            options.Extractor.OptionPrefix = prefix;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The option prefix cannot be more than 3 Unicode characters. option_prefix={prefix}");
        }

        [DataTestMethod]
        [DataRow("@")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task OptionSeparatorAndOptionAliasPrefixCannotBeSameAsync(string separator)
        {
            options.Extractor.OptionValueSeparator = separator;
            options.Extractor.OptionPrefix = ":";
            options.Extractor.OptionAliasPrefix = separator;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The option separator and option alias prefix cannot be same. separator={separator}");
        }

        [DataTestMethod]
        [DataRow("@")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task OptionSeparatorAndOptionPrefixCannotBeSameAsync(string separator)
        {
            options.Extractor.OptionValueSeparator = separator;
            options.Extractor.OptionPrefix = separator;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The option separator and option prefix cannot be same. separator={separator}");
        }

        [TestMethod]
        public async Task OptionSeparatorCannotBeNullOrEmptyAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Extractor.OptionValueSeparator = null;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, "The option separator cannot be null or empty.");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            options.Extractor.OptionValueSeparator = "";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, "The option separator cannot be null or empty.");
        }

        [DataTestMethod]
        [DataRow(" ")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandSeparatorAndOptionPrefixCannotBeSameAsync(string separator)
        {
            options.Extractor.Separator = separator;
            options.Extractor.OptionPrefix = separator;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The command separator and option prefix cannot be same. separator={separator}");
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
        public async Task DefaultOptionConfiguredButProviderNotConfiguredShouldThrow()
        {
            options.Extractor.DefaultOption = true;

            CommandExtractorContext context = new(new CommandRoute("id1", "prefix6_defaultarg"));

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, "The command default option provider is missing in the service collection. provider_type=IDefaultOptionProvider");
        }

        [TestMethod]
        [WriteDocumentation]
        public async Task LoggerIndentLessThanOrEqualToZeroShouldThtow()
        {
            options.Logging.LoggerIndent = -3;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, "The terminal logger indent cannot be less than or equal to zero. logger_indent=-3");

            options.Logging.LoggerIndent = 0;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, "The terminal logger indent cannot be less than or equal to zero. logger_indent=0");
        }

        [TestMethod]
        public async Task DefaultValueConfiguredButProviderNotConfiguredShouldThrow()
        {
            options.Extractor.DefaultOptionValue = true;

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, "The option default value provider is missing in the service collection. provider_type=IDefaultOptionValueProvider");
        }

        [TestMethod]
        public async Task WithInStringCannotBeSameAsArgAliasPrefix()
        {
            // Make sure command separator is different so we can fail for option separator below.
            options.Extractor.OptionPrefix = "#";
            options.Extractor.OptionValueWithIn = "^";
            options.Extractor.OptionAliasPrefix = "^";

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The string with_in token and option alias prefix cannot be same. with_in=^");
        }

        [TestMethod]
        public async Task WithInStringCannotBeSameAsArgPrefix()
        {
            // Make sure command separator is different so we can fail for option separator below.
            options.Extractor.OptionPrefix = "^";
            options.Extractor.OptionValueWithIn = "^";

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The string with_in token and option prefix cannot be same. with_in=^");
        }

        [TestMethod]
        public async Task WithInStringCannotBeSameAsArgSeparator()
        {
            // Make sure command separator is different so we can fail for option separator below.
            options.Extractor.OptionValueSeparator = "^";
            options.Extractor.OptionValueWithIn = "^";

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The string with_in token and option separator cannot be same. with_in=^");
        }

        [TestMethod]
        public async Task WithInStringCannotBeSameAsSeparator()
        {
            // Make sure command separator is different so we can fail for option separator below.
            options.Extractor.Separator = "^";
            options.Extractor.OptionValueWithIn = "^";

            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The string with_in token and separator cannot be same. with_in=^");
        }

        [TestMethod]
        public async Task WithInStringCannotBeWhitespace()
        {
            // Make sure command separator is different so we can fail for option separator below.
            options.Extractor.OptionValueWithIn = "   ";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => optionsChecker.CheckAsync(options), Errors.InvalidConfiguration, $"The string with_in token cannot be whitespace.");
        }

        private readonly OptionExtractor argExtractor;
        private readonly ICommandStoreHandler commands;
        private readonly IDefaultOptionProvider defaultArgProvider = null!;
        private readonly IDefaultOptionValueProvider defaultArgValueProvider = null!;
        private readonly IHost host;
        private readonly IHostBuilder hostBuilder;
        private readonly TerminalOptions options;
        private readonly IConfigurationOptionsChecker optionsChecker;
        private readonly ITextHandler textHandler;
    }
}