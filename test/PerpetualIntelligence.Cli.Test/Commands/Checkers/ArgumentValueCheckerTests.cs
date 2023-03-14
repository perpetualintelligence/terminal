/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Mappers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Shared.Attributes.Validation;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    [TestClass]
    public class ArgumentValueCheckerTests : InitializerTests
    {
        public ArgumentValueCheckerTests() : base(TestLogger.Create<ArgumentValueCheckerTests>())
        {
        }

        [TestMethod]
        public async Task MapperFailureShouldErrorAsync()
        {
            // Any failure, we just want to test that mapper failure is correclty returned
            OptionDescriptor identity = new("arg1", (DataType)int.MaxValue, "desc1");
            Option value = new(identity, 23.69);

            ArgumentCheckerContext context = new(identity, value);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.UnsupportedArgument, "The argument data type is not supported. argument=arg1 data_type=2147483647");
        }

        [TestMethod]
        public async Task NullArgumentValueShouldErrorAsync()
        {
            OptionDescriptor identity = new("arg1", DataType.Text, "desc1");
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Option value = new(identity, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            ArgumentCheckerContext context = new(identity, value);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument value cannot be null. argument=arg1");
        }

        [TestMethod]
        public async Task StrictTypeChecking_InvalidMappedType_ButConvertible_ShouldNotErrorAsync()
        {
            options.Checker.StrictArgumentValueType = true;

            // Value is double, but we can convert it so this should not error.
            OptionDescriptor identity = new("arg1", DataType.Text, "desc1");
            Option value = new(identity, 23.69);

            ArgumentCheckerContext context = new(identity, value);
            await checker.CheckAsync(context);

            // Check converted
            Assert.AreEqual("23.69", value.Value);
            Assert.IsInstanceOfType(value.Value, typeof(string));
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabled_InvalidMappedType_ButConvertible_ShouldNotErrorAsync()
        {
            options.Checker.StrictArgumentValueType = false;

            // Value is double, strict checking is disabled so we will not convert it
            OptionDescriptor identity = new("arg1", DataType.Text, "desc1");
            Option value = new(identity, 23.69);

            ArgumentCheckerContext context = new(identity, value);
            await checker.CheckAsync(context);

            // Check not converted
            Assert.AreEqual(23.69, value.Value);
            Assert.IsInstanceOfType(value.Value, typeof(double));
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabledNotSupportedValueShouldNotErrorAsync()
        {
            options.Checker.StrictArgumentValueType = false;

            OptionDescriptor identity = new("arg1", DataType.Text, "desc1") { ValueCheckers = new[] { new DataValidationArgumentValueChecker(new OneOfAttribute("test1", "test2")) } };
            Option value = new(identity, "test3");

            ArgumentCheckerContext context = new(identity, value);
            await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabledSystemTypeMatchAndDataValidationFailShouldNotErrorAsync()
        {
            options.Checker.StrictArgumentValueType = false;

            OptionDescriptor identity = new("arg1", DataType.CreditCard, "desc1") { ValueCheckers = new[] { new DataValidationArgumentValueChecker(new CreditCardAttribute()) } };
            Option value = new(identity, "invalid_4242424242424242");

            ArgumentCheckerContext context = new ArgumentCheckerContext(identity, value);
            await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task StrictTypeCheckingNotSupportedValueShouldErrorAsync()
        {
            options.Checker.StrictArgumentValueType = true;

            OptionDescriptor identity = new("arg1", DataType.Text, "desc1") { ValueCheckers = new[] { new DataValidationArgumentValueChecker(new OneOfAttribute("test1", "test2")) } };
            Option value = new(identity, "test3");

            ArgumentCheckerContext context = new(identity, value);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument value is not valid. argument=arg1 value=test3 info=The field value must be one of the valid values.");
        }

        [TestMethod]
        public async Task StrictTypeCheckingSystemTypeMatchAndDataValidationFailShouldErrorAsync()
        {
            options.Checker.StrictArgumentValueType = true;

            OptionDescriptor identity = new("arg1", DataType.CreditCard, "desc1") { ValueCheckers = new[] { new DataValidationArgumentValueChecker(new CreditCardAttribute()) } };
            Option value = new(identity, "invalid_4242424242424242");

            ArgumentCheckerContext context = new ArgumentCheckerContext(identity, value);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument value is not valid. argument=arg1 value=invalid_4242424242424242 info=The Option field is not a valid credit card number.");
        }

        [TestMethod]
        public async Task SupportedValueShouldNotErrorAsync()
        {
            OptionDescriptor identity = new("arg1", DataType.Text, "desc1") { ValueCheckers = new[] { new DataValidationArgumentValueChecker(new OneOfAttribute("test1", "test2")) } };
            Option value = new(identity, "test2");

            ArgumentCheckerContext context = new(identity, value);
            ArgumentCheckerResult result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task SystemTypeMatchAndDataValidationSuccessShouldNotErrorAsync()
        {
            OptionDescriptor identity = new("arg1", DataType.CreditCard, "desc1");
            Option value = new(identity, "4242424242424242");

            ArgumentCheckerContext context = new ArgumentCheckerContext(identity, value);
            var result = await checker.CheckAsync(context);
            Assert.AreEqual(typeof(string), result.MappedType);
        }

        [TestMethod]
        public void ValidationAttributeCheckFailShouldError()
        {
        }

        protected override void OnTestInitialize()
        {
            options = MockCliOptions.New();
            mapper = new DataAnnotationsArgumentDataTypeMapper(options, TestLogger.Create<DataAnnotationsArgumentDataTypeMapper>());
            checker = new ArgumentChecker(mapper, options, TestLogger.Create<ArgumentChecker>());
        }

        private IArgumentChecker checker = null!;
        private IArgumentDataTypeMapper mapper = null!;
        private CliOptions options = null!;
    }
}