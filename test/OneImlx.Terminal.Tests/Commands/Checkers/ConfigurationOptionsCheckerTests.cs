/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Test.FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Checkers
{
    public class ConfigurationOptionsCheckerTests
    {
        public ConfigurationOptionsCheckerTests()
        {
            options = MockTerminalOptions.NewLegacyOptions();
            textHandler = new TerminalUnicodeTextHandler();

            hostBuilder = Host.CreateDefaultBuilder().ConfigureServices(services =>
            {
                services.AddSingleton(textHandler);
                services.AddSingleton(options);
            });
            host = hostBuilder.Start();

            optionsChecker = new ConfigurationOptionsChecker(host.Services);
        }

        [Theory]
        [InlineData(' ')]
        [InlineData('~')]
        [InlineData('#')]
        [InlineData('s')]
        [InlineData('ö')]
        [InlineData('म')]
        [InlineData('女')]
        public async Task CommandSeparatorAndOptionPrefixCannotBeSameAsync(char separator)
        {
            options.Parser.Separator = separator;
            options.Parser.OptionPrefix = separator;

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The command separator and option prefix cannot be same. separator={separator}");
        }

        [Fact]
        public async Task CommandSeparatorCannotBeNullOrEmptyAsync()
        {
            Func<Task> func = async () => await optionsChecker.CheckAsync(options);

            options.Parser.Separator = default;
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The command separator cannot be null or empty.");

            options.Parser.Separator = '\0';
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The command separator cannot be null or empty.");
        }

        [Fact]
        public async Task LinkedToRoot_Requires_Terminal_Name()
        {
            options.Driver.Enabled = true;
            Func<Task> func = async () => await optionsChecker.CheckAsync(options);

            // Driver Name null or whitespace with enabled driver.
            options.Driver.Name = "";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The name is required if terminal root is a driver.");

            options.Driver.Name = null;
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The name is required if terminal root is a driver.");

            options.Driver.Name = "   ";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The name is required if terminal root is a driver.");

            // Driver Name not null or whitespace with disabled driver.
            options.Driver.Enabled = false;
            options.Driver.Name = "";
            await func.Invoke();

            options.Driver.Name = null;
            await func.Invoke();

            options.Driver.Name = "   ";
            await func.Invoke();
        }

        [Fact]
        public async Task OptionPrefixCannotBeDefualtOrEmpty()
        {
            options.Parser.OptionPrefix = '\0';
            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The option prefix cannot be default.");

            options.Parser.OptionPrefix = default;
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The option prefix cannot be default.");
        }

        [Fact]
        public async Task OptionSeparatorAndOptionPrefixCannotBeSameAsync()
        {
            char separator = '@';
            options.Parser.OptionValueSeparator = separator;
            options.Parser.OptionPrefix = separator;

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The option separator and option prefix cannot be same. separator={separator}");
        }

        [Fact]
        public async Task OptionSeparatorCannotBeNullOrEmptyAsync()
        {
            Func<Task> func = async () => await optionsChecker.CheckAsync(options);

            options.Parser.OptionValueSeparator = default;
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The option separator cannot be null or empty.");

            options.Parser.OptionValueSeparator = '\0';
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The option separator cannot be null or empty.");
        }

        [Fact]
        public async Task Terminal_Id_Is_Required()
        {
            Func<Task> func = async () => await optionsChecker.CheckAsync(options);

            options.Id = "";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The terminal identifier is required.");

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Id = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The terminal identifier is required.");

            options.Id = "   ";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The terminal identifier is required.");

            options.Id = "asasd";
            await func.Invoke();
        }

        [Fact]
        public async Task ValueDelimiterStringCannotBeNullOrWhitespace()
        {
            Func<Task> func = async () => await optionsChecker.CheckAsync(options);

            // Make sure command separator is different so we can fail for option separator below.
            options.Parser.ValueDelimiter = ' ';
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The value delimiter cannot be null or whitespace.");

            options.Parser.ValueDelimiter = default;
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The value delimiter cannot be null or whitespace.");
        }

        [Fact]
        public async Task ValueDelimiterStringCannotBeSameAsOptionPrefix()
        {
            // Make sure command separator is different so we can fail for option separator below.
            options.Parser.OptionPrefix = '^';
            options.Parser.ValueDelimiter = '^';

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The value delimiter cannot be same as the option prefix. delimiter=^");
        }

        [Fact]
        public async Task ValueDelimiterStringCannotBeSameAsOptionValueSeparator()
        {
            // Make sure command separator is different so we can fail for option separator below.
            options.Parser.OptionValueSeparator = '^';
            options.Parser.ValueDelimiter = '^';

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The value delimiter cannot be same as the option value separator. delimiter=^");
        }

        [Fact]
        public async Task ValueDelimiterStringCannotBeSameAsSeparator()
        {
            // Make sure command separator is different so we can fail for option separator below.
            options.Parser.Separator = '^';
            options.Parser.ValueDelimiter = '^';

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription($"The value delimiter cannot be same as the separator. delimiter=^");
        }

        private readonly IHost host;
        private readonly IHostBuilder hostBuilder;
        private readonly TerminalOptions options;
        private readonly IConfigurationOptionsChecker optionsChecker;
        private readonly ITerminalTextHandler textHandler;
    }
}
