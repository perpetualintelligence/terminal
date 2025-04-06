/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentAssertions;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Test.FluentAssertions;
using Xunit;

namespace OneImlx.Terminal.Commands.Checkers
{
    public class ConfigurationOptionsCheckerTests
    {
        public ConfigurationOptionsCheckerTests()
        {
            options = MockTerminalOptions.NewLegacyOptions();
            textHandler = new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);

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
        public async Task Invalid_Licensing_Deployment_Throws()
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            options.Licensing.Deployment = "invalid_deployment";
#pragma warning restore CS8601 // Possible null reference assignment.

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The license deployment is not valid. deployment=invalid_deployment");
        }

        [Fact]
        public async Task LinkedToRoot_Requires_Terminal_Name()
        {
            options.Driver.Enabled = true;
            Func<Task> func = async () => await optionsChecker.CheckAsync(options);

            // Driver Name null or whitespace with enabled driver.
            options.Driver.RootId = "";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The root is required for driver programs.");

            options.Driver.RootId = null;
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The root is required for driver programs.");

            options.Driver.RootId = "   ";
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The root is required for driver programs.");

            // Driver Name not null or whitespace with disabled driver.
            options.Driver.Enabled = false;
            options.Driver.RootId = "";
            await func.Invoke();

            options.Driver.RootId = null;
            await func.Invoke();

            options.Driver.RootId = "   ";
            await func.Invoke();
        }

        [Theory]
        [InlineData("  ")]
        [InlineData("")]
        [InlineData(null)]
        public async Task Missing_Licensing_Deployment_Throws(string? deployment)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            options.Licensing.Deployment = deployment;
#pragma warning restore CS8601 // Possible null reference assignment.

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The license deployment is not specified.");
        }

        [Theory]
        [InlineData("  ")]
        [InlineData("")]
        [InlineData(null)]
        public async Task Missing_Licensing_Plan_Throws(string? plan)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            options.Licensing.LicensePlan = plan;
#pragma warning restore CS8601 // Possible null reference assignment.

            Func<Task> func = async () => await optionsChecker.CheckAsync(options);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidConfiguration).WithErrorDescription("The license plan is not specified.");
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
