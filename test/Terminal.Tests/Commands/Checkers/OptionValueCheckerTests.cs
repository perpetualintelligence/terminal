/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Shared.Attributes.Validation;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Commands.Mappers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
{
    [TestClass]
    public class OptionValueCheckerTests : InitializerTests
    {
        public OptionValueCheckerTests() : base(TestLogger.Create<OptionValueCheckerTests>())
        {
        }

        [TestMethod]
        public async Task MapperFailureShouldErrorAsync()
        {
            // Any failure, we just want to test that mapper failure is correctly returned
            OptionDescriptor identity = new("opt1", "invalid_dt", "desc1", OptionFlags.None);
            Option value = new(identity, "non int value");

            OptionCheckerContext context = new(value);
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => checker.CheckOptionAsync(context), TerminalErrors.UnsupportedOption, "The option data type is not supported. option=opt1 data_type=invalid_dt");
        }

        [TestMethod]
        public async Task NullOptionValueShouldErrorAsync()
        {
            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Option value = new(identity, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            OptionCheckerContext context = new(value);
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => checker.CheckOptionAsync(context), TerminalErrors.InvalidOption, "The option value cannot be null. option=opt1");
        }

        [TestMethod]
        public async Task StrictTypeChecking_InvalidMappedType_ButConvertible_ShouldNotErrorAsync()
        {
            options.Checker.StrictValueType = true;

            // Value is double, but we can convert it so this should not error.
            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None);
            Option value = new(identity, 23.69);

            OptionCheckerContext context = new(value);
            await checker.CheckOptionAsync(context);

            // Check converted
            Assert.AreEqual("23.69", value.Value);
            Assert.IsInstanceOfType(value.Value, typeof(string));
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabled_InvalidMappedType_ButConvertible_ShouldNotErrorAsync()
        {
            options.Checker.StrictValueType = false;

            // Value is double, strict checking is disabled so we will not convert it
            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None);
            Option value = new(identity, 23.69);

            OptionCheckerContext context = new(value);
            await checker.CheckOptionAsync(context);

            // Check not converted
            Assert.AreEqual(23.69, value.Value);
            Assert.IsInstanceOfType(value.Value, typeof(double));
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabled_NotSupportedValue_ShouldErrorAsync()
        {
            options.Checker.StrictValueType = false;

            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None) { ValueCheckers = new[] { new DataValidationValueChecker<Option>(new OneOfAttribute("test1", "test2")) } };
            Option value = new(identity, "test3");

            OptionCheckerContext context = new(value);
            Func<Task> func = async () => await checker.CheckOptionAsync(context);
            await func.Should().ThrowAsync<TerminalException>().WithMessage("The option value is not valid. option=opt1 value=test3 info=The field value must be one of the valid values.");
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabled_SystemTypeMatch_AndDataValidationFail_ShouldErrorAsync()
        {
            options.Checker.StrictValueType = false;

            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None) { ValueCheckers = new[] { new DataValidationValueChecker<Option>(new CreditCardAttribute()) } };
            Option value = new(identity, "invalid_4242424242424242");

            OptionCheckerContext context = new(value);
            Func<Task> func = async () => await checker.CheckOptionAsync(context);
            await func.Should().ThrowAsync<TerminalException>().WithMessage("The option value is not valid. option=opt1 value=invalid_4242424242424242 info=The Option field is not a valid credit card number.");
        }

        [TestMethod]
        public async Task StrictTypeChecking_NotSupportedValue_ShouldErrorAsync()
        {
            options.Checker.StrictValueType = true;

            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None) { ValueCheckers = new[] { new DataValidationValueChecker<Option>(new OneOfAttribute("test1", "test2")) } };
            Option value = new(identity, "test3");

            OptionCheckerContext context = new(value);
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => checker.CheckOptionAsync(context), TerminalErrors.InvalidOption, "The option value is not valid. option=opt1 value=test3 info=The field value must be one of the valid values.");
        }

        [TestMethod]
        public async Task StrictTypeCheckingSystemTypeMatchAndDataValidationFailShouldErrorAsync()
        {
            options.Checker.StrictValueType = true;

            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None) { ValueCheckers = new[] { new DataValidationValueChecker<Option>(new CreditCardAttribute()) } };
            Option value = new(identity, "invalid_4242424242424242");

            OptionCheckerContext context = new(value);
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => checker.CheckOptionAsync(context), TerminalErrors.InvalidOption, "The option value is not valid. option=opt1 value=invalid_4242424242424242 info=The Option field is not a valid credit card number.");
        }

        [TestMethod]
        public async Task SupportedValueShouldNotErrorAsync()
        {
            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None) { ValueCheckers = new[] { new DataValidationValueChecker<Option>(new OneOfAttribute("test1", "test2")) } };
            Option value = new(identity, "test2");

            OptionCheckerContext context = new(value);
            await checker.CheckOptionAsync(context);
        }

        [TestMethod]
        public async Task SystemTypeMatchAndDataValidationSuccessShouldNotErrorAsync()
        {
            OptionDescriptor identity = new("opt1", nameof(String), "desc1", OptionFlags.None);
            Option value = new(identity, "4242424242424242");

            OptionCheckerContext context = new(value);
            var result = await checker.CheckOptionAsync(context);
            Assert.AreEqual(typeof(string), result.MappedType);
        }

        protected override void OnTestInitialize()
        {
            options = MockTerminalOptions.NewLegacyOptions();
            mapper = new DataTypeMapper<Option>(options, TestLogger.Create<DataTypeMapper<Option>>());
            checker = new OptionChecker(mapper, options);
        }

        private IOptionChecker checker = null!;
        private IDataTypeMapper<Option> mapper = null!;
        private TerminalOptions options = null!;
    }
}