/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Mappers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Attributes.Validation;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    [TestClass]
    public class ArgumentValueCheckerTests : OneImlxLogTest
    {
        public ArgumentValueCheckerTests() : base(TestLogger.Create<ArgumentValueCheckerTests>())
        {
        }

        [TestMethod]
        public async Task InvalidMappedTypeShouldErrorAsync()
        {
            ArgumentIdentity identity = new("arg1", DataType.Text);
            Argument value = new("arg1", 23.69, DataType.Text);

            ArgumentCheckerContext context = new ArgumentCheckerContext(identity, value);
            var result = await checker.CheckAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument value does not match the mapped type. argument=arg1 type=System.String data_type=Text value_type=Double value=23.69");
        }

        [TestMethod]
        public async Task MapperFailureShouldErrorAsync()
        {
            // Any failure, we just want to test that mapper failure is correclty returned
            ArgumentIdentity identity = new("arg1", (DataType)int.MaxValue);
            Argument value = new("arg1", 23.69, (DataType)int.MaxValue);

            ArgumentCheckerContext context = new ArgumentCheckerContext(identity, value);
            var result = await checker.CheckAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.UnsupportedArgument, "The argument data type is not supported. argument=arg1 data_type=2147483647");
        }

        [TestMethod]
        public async Task NotSupportedValueShouldErrorAsync()
        {
            ArgumentIdentity identity = new("arg1", DataType.Text, validationAttributes: new[] { new OneOfAttribute("test1", "test2") });
            Argument value = new("arg1", "test3", DataType.Text);

            ArgumentCheckerContext context = new ArgumentCheckerContext(identity, value);
            var result = await checker.CheckAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument value is not valid. argument=arg1 value=test3 additional_info=The field value must be one of the valid values.");
        }

        [TestMethod]
        public async Task NullArgumentValueShouldErrorAsync()
        {
            ArgumentIdentity identity = new("arg1", DataType.Text);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Argument value = new("arg1", null, DataType.Text);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            ArgumentCheckerContext context = new ArgumentCheckerContext(identity, value);
            var result = await checker.CheckAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument value cannot be null. argument=arg1");
        }

        [TestMethod]
        public async Task SupportedValueShouldNotErrorAsync()
        {
            ArgumentIdentity identity = new("arg1", DataType.Text, validationAttributes: new[] { new OneOfAttribute("test1", "test2") });
            Argument value = new("arg1", "test2", DataType.Text);

            ArgumentCheckerContext context = new ArgumentCheckerContext(identity, value);
            var result = await checker.CheckAsync(context);
            Assert.IsFalse(result.IsError);
        }

        [TestMethod]
        public async Task SystemTypeMatchAndDataValidationFailShouldErrorAsync()
        {
            ArgumentIdentity identity = new("arg1", DataType.CreditCard, validationAttributes: new[] { new CreditCardAttribute() });
            Argument value = new("arg1", "invalid_4242424242424242", DataType.CreditCard);

            ArgumentCheckerContext context = new ArgumentCheckerContext(identity, value);
            var result = await checker.CheckAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument value is not valid. argument=arg1 value=invalid_4242424242424242 additional_info=The Argument field is not a valid credit card number.");
        }

        //[TestMethod]
        //public async Task SystemTypeMatchAndDataValidationDataTypeStdFailShouldErrorAsync()
        //{
        //    ArgumentIdentity identity = new("arg1", DataType.Password);
        //    Argument value = new("arg1", "-3663", DataType.Password);

        //    ArgumentValueCheckerContext context = new ArgumentValueCheckerContext(identity, value);
        //    var result = await checker.CheckAsync(context);
        //    TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument value is not valid. argument=arg1 value=invalid_4242424242424242 additional_info=The Argument field is not a valid credit card number.");
        //}

        //[TestMethod]
        //public async Task SystemTypeMatchAndDataValidationCustomDataTypeStdFailShouldErrorAsync()
        //{
        //    ArgumentIdentity identity = new("arg1", DataType.Password);
        //    Argument value = new("arg1", "-3663", DataType.Password);

        //    ArgumentValueCheckerContext context = new ArgumentValueCheckerContext(identity, value);
        //    var result = await checker.CheckAsync(context);
        //    TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument value is not valid. argument=arg1 value=invalid_4242424242424242 additional_info=The Argument field is not a valid credit card number.");
        //}

        [TestMethod]
        public async Task SystemTypeMatchAndDataValidationSuccessShouldNotErrorAsync()
        {
            ArgumentIdentity identity = new("arg1", DataType.CreditCard);
            Argument value = new("arg1", "4242424242424242", DataType.CreditCard);

            ArgumentCheckerContext context = new ArgumentCheckerContext(identity, value);
            var result = await checker.CheckAsync(context);
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(typeof(string), result.MappedSystemType);
        }

        [TestMethod]
        public void ValidationAttributeCheckFailShoudError()
        {
        }

        protected override void OnTestInitialize()
        {
            options = MockCliOptions.New();
            mapper = new DataAnnotationsTypeMapper(options, TestLogger.Create<DataAnnotationsTypeMapper>());
            checker = new DataAnnotationsArgumentChecker(mapper, options, TestLogger.Create<DataAnnotationsArgumentChecker>());
        }

        private IArgumentChecker checker = null!;
        private IArgumentMapper mapper = null!;
        private CliOptions options = null!;
    }
}
