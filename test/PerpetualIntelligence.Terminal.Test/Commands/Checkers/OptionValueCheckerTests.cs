/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Commands.Mappers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Shared.Attributes.Validation;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
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
            // Any failure, we just want to test that mapper failure is correclty returned
            OptionDescriptor identity = new("arg1", (DataType)int.MaxValue, "desc1");
            Option value = new(identity, 23.69);

            OptionCheckerContext context = new(identity, value);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.UnsupportedOption, "The option data type is not supported. option=arg1 data_type=2147483647");
        }

        [TestMethod]
        public async Task NullOptionValueShouldErrorAsync()
        {
            OptionDescriptor identity = new("arg1", DataType.Text, "desc1");
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Option value = new(identity, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            OptionCheckerContext context = new(identity, value);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidOption, "The option value cannot be null. option=arg1");
        }

        [TestMethod]
        public async Task StrictTypeChecking_InvalidMappedType_ButConvertible_ShouldNotErrorAsync()
        {
            options.Checker.StrictOptionValueType = true;

            // Value is double, but we can convert it so this should not error.
            OptionDescriptor identity = new("arg1", DataType.Text, "desc1");
            Option value = new(identity, 23.69);

            OptionCheckerContext context = new(identity, value);
            await checker.CheckAsync(context);

            // Check converted
            Assert.AreEqual("23.69", value.Value);
            Assert.IsInstanceOfType(value.Value, typeof(string));
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabled_InvalidMappedType_ButConvertible_ShouldNotErrorAsync()
        {
            options.Checker.StrictOptionValueType = false;

            // Value is double, strict checking is disabled so we will not convert it
            OptionDescriptor identity = new("arg1", DataType.Text, "desc1");
            Option value = new(identity, 23.69);

            OptionCheckerContext context = new(identity, value);
            await checker.CheckAsync(context);

            // Check not converted
            Assert.AreEqual(23.69, value.Value);
            Assert.IsInstanceOfType(value.Value, typeof(double));
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabledNotSupportedValueShouldNotErrorAsync()
        {
            options.Checker.StrictOptionValueType = false;

            OptionDescriptor identity = new("arg1", DataType.Text, "desc1") { ValueCheckers = new[] { new DataValidationOptionValueChecker(new OneOfAttribute("test1", "test2")) } };
            Option value = new(identity, "test3");

            OptionCheckerContext context = new(identity, value);
            await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabledSystemTypeMatchAndDataValidationFailShouldNotErrorAsync()
        {
            options.Checker.StrictOptionValueType = false;

            OptionDescriptor identity = new("arg1", DataType.CreditCard, "desc1") { ValueCheckers = new[] { new DataValidationOptionValueChecker(new CreditCardAttribute()) } };
            Option value = new(identity, "invalid_4242424242424242");

            OptionCheckerContext context = new(identity, value);
            await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task StrictTypeCheckingNotSupportedValueShouldErrorAsync()
        {
            options.Checker.StrictOptionValueType = true;

            OptionDescriptor identity = new("arg1", DataType.Text, "desc1") { ValueCheckers = new[] { new DataValidationOptionValueChecker(new OneOfAttribute("test1", "test2")) } };
            Option value = new(identity, "test3");

            OptionCheckerContext context = new(identity, value);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidOption, "The option value is not valid. option=arg1 value=test3 info=The field value must be one of the valid values.");
        }

        [TestMethod]
        public async Task StrictTypeCheckingSystemTypeMatchAndDataValidationFailShouldErrorAsync()
        {
            options.Checker.StrictOptionValueType = true;

            OptionDescriptor identity = new("arg1", DataType.CreditCard, "desc1") { ValueCheckers = new[] { new DataValidationOptionValueChecker(new CreditCardAttribute()) } };
            Option value = new(identity, "invalid_4242424242424242");

            OptionCheckerContext context = new(identity, value);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidOption, "The option value is not valid. option=arg1 value=invalid_4242424242424242 info=The Option field is not a valid credit card number.");
        }

        [TestMethod]
        public async Task SupportedValueShouldNotErrorAsync()
        {
            OptionDescriptor identity = new("arg1", DataType.Text, "desc1") { ValueCheckers = new[] { new DataValidationOptionValueChecker(new OneOfAttribute("test1", "test2")) } };
            Option value = new(identity, "test2");

            OptionCheckerContext context = new(identity, value);
            await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task SystemTypeMatchAndDataValidationSuccessShouldNotErrorAsync()
        {
            OptionDescriptor identity = new("arg1", DataType.CreditCard, "desc1");
            Option value = new(identity, "4242424242424242");

            OptionCheckerContext context = new(identity, value);
            var result = await checker.CheckAsync(context);
            Assert.AreEqual(typeof(string), result.MappedType);
        }

        [TestMethod]
        public void ValidationAttributeCheckFailShouldError()
        {
        }

        protected override void OnTestInitialize()
        {
            options = MockTerminalOptions.New();
            mapper = new DataAnnotationsOptionDataTypeMapper(options, TestLogger.Create<DataAnnotationsOptionDataTypeMapper>());
            checker = new OptionChecker(mapper, options);
        }

        private IOptionChecker checker = null!;
        private IOptionDataTypeMapper mapper = null!;
        private TerminalOptions options = null!;
    }
}