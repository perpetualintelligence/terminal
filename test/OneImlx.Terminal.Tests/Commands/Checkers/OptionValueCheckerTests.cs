/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Attributes.Validation;
using OneImlx.Terminal.Commands.Mappers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Test.FluentAssertions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Checkers
{
    public class OptionValueCheckerTests
    {
        public OptionValueCheckerTests()
        {
            options = MockTerminalOptions.NewLegacyOptions();
            mapper = new DataTypeMapper<Option>(options, new LoggerFactory().CreateLogger<DataTypeMapper<Option>>());
            checker = new OptionChecker(mapper, options);
        }

        [Fact]
        public async Task MapperFailureShouldErrorAsync()
        {
            // Any failure, we just want to test that mapper failure is correctly returned
            OptionDescriptor identity = new("opt1", "invalid_dt", "desc1", OptionFlags.None);
            Option value = new(identity, "non int value");

            Func<Task> func = () => checker.CheckOptionAsync(value);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.UnsupportedOption).WithErrorDescription("The option data type is not supported. option=opt1 data_type=invalid_dt");
        }

        [Fact]
        public async Task NullOptionValueShouldErrorAsync()
        {
            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Option value = new(identity, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            Func<Task> func = () => checker.CheckOptionAsync(value);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidOption).WithErrorDescription("The option value cannot be null. option=opt1");
        }

        [Fact]
        public async Task StrictTypeChecking_InvalidMappedType_ButConvertible_ShouldNotErrorAsync()
        {
            options.Checker.StrictValueType = true;

            // Value is double, but we can convert it so this should not error.
            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None);
            Option value = new(identity, 23.69);

            await checker.CheckOptionAsync(value);

            // Check converted
            value.Value.Should().Be("23.69");
            value.Value.Should().BeOfType<string>();
        }

        [Fact]
        public async Task StrictTypeChecking_NotSupportedValue_ShouldErrorAsync()
        {
            options.Checker.StrictValueType = true;

            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None) { ValueCheckers = [new DataValidationValueChecker<Option>(new OneOfAttribute("test1", "test2"))] };
            Option value = new(identity, "test3");

            Func<Task> func = async () => await checker.CheckOptionAsync(value);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidOption).WithErrorDescription("The option value is not valid. option=opt1 value=test3 info=The field value must be one of the valid values.");
        }

        [Fact]
        public async Task StrictTypeCheckingDisabled_InvalidMappedType_ButConvertible_ShouldNotErrorAsync()
        {
            options.Checker.StrictValueType = false;

            // Value is double, strict checking is disabled so we will not convert it
            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None);
            Option value = new(identity, 23.69);

            await checker.CheckOptionAsync(value);

            // Check not converted
            value.Value.Should().Be(23.69);
            value.Value.Should().BeOfType<double>();
        }

        [Fact]
        public async Task StrictTypeCheckingDisabled_NotSupportedValue_ShouldErrorAsync()
        {
            options.Checker.StrictValueType = false;

            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None) { ValueCheckers = [new DataValidationValueChecker<Option>(new OneOfAttribute("test1", "test2"))] };
            Option value = new(identity, "test3");

            Func<Task> func = async () => await checker.CheckOptionAsync(value);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidOption).WithErrorDescription("The option value is not valid. option=opt1 value=test3 info=The field value must be one of the valid values.");
        }

        [Fact]
        public async Task StrictTypeCheckingDisabled_SystemTypeMatch_AndDataValidationFail_ShouldErrorAsync()
        {
            options.Checker.StrictValueType = false;

            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None) { ValueCheckers = [new DataValidationValueChecker<Option>(new CreditCardAttribute())] };
            Option value = new(identity, "invalid_4242424242424242");

            Func<Task> func = async () => await checker.CheckOptionAsync(value);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidOption).WithErrorDescription("The option value is not valid. option=opt1 value=invalid_4242424242424242 info=The Option field is not a valid credit card number.");
        }

        [Fact]
        public async Task StrictTypeCheckingSystemTypeMatchAndDataValidationFailShouldErrorAsync()
        {
            options.Checker.StrictValueType = true;

            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None) { ValueCheckers = [new DataValidationValueChecker<Option>(new CreditCardAttribute())] };
            Option value = new(identity, "invalid_4242424242424242");

            Func<Task> func = async () => await checker.CheckOptionAsync(value);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidOption).WithErrorDescription("The option value is not valid. option=opt1 value=invalid_4242424242424242 info=The Option field is not a valid credit card number.");
        }

        [Fact]
        public async Task SupportedValueShouldNotErrorAsync()
        {
            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None) { ValueCheckers = [new DataValidationValueChecker<Option>(new OneOfAttribute("test1", "test2"))] };
            Option value = new(identity, "test2");

            await checker.CheckOptionAsync(value);
        }

        [Fact]
        public async Task SystemTypeMatchAndDataValidationSuccessShouldNotErrorAsync()
        {
            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None);
            Option value = new(identity, "4242424242424242");

            var result = await checker.CheckOptionAsync(value);
            result.MappedType.Should().Be(typeof(string));
        }

        private readonly IOptionChecker checker = null!;
        private readonly IDataTypeMapper<Option> mapper = null!;
        private readonly TerminalOptions options = null!;
    }
}
