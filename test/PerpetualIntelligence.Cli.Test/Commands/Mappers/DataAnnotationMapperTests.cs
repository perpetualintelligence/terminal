/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Mappers
{
    [TestClass]
    public class DataAnnotationMapperTests : OneImlxLogTest
    {
        public DataAnnotationMapperTests() : base(TestLogger.Create<DataAnnotationMapperTests>())
        {
        }

        [DataTestMethod]
        [DataRow(DataType.CreditCard, typeof(string), typeof(CreditCardAttribute), null)]
        [DataRow(DataType.Currency, typeof(string), typeof(DataTypeAttribute), null)]
        [DataRow(DataType.Date, typeof(DateTime), typeof(DataTypeAttribute), null)]
        [DataRow(DataType.DateTime, typeof(DateTime), typeof(DataTypeAttribute), null)]
        [DataRow(DataType.Duration, typeof(TimeSpan), typeof(DataTypeAttribute), null)]
        [DataRow(DataType.EmailAddress, typeof(string), typeof(EmailAddressAttribute), null)]
        [DataRow(DataType.Html, typeof(string), typeof(DataTypeAttribute), null)]
        [DataRow(DataType.ImageUrl, typeof(Uri), typeof(UrlAttribute), null)]
        [DataRow(DataType.MultilineText, typeof(string), typeof(DataTypeAttribute), null)]
        [DataRow(DataType.Password, typeof(string), typeof(DataTypeAttribute), null)]
        [DataRow(DataType.PhoneNumber, typeof(string), typeof(PhoneAttribute), null)]
        [DataRow(DataType.PostalCode, typeof(string), typeof(DataTypeAttribute), null)]
        [DataRow(DataType.Text, typeof(string), typeof(DataTypeAttribute), null)]
        [DataRow(DataType.Time, typeof(DateTime), typeof(DataTypeAttribute), null)]
        [DataRow(DataType.Upload, typeof(string), typeof(DataTypeAttribute), null)]
        [DataRow(DataType.Url, typeof(Uri), typeof(UrlAttribute), null)]
        [DataRow(DataType.Custom, typeof(bool), null, nameof(Boolean))]
        [DataRow(DataType.Custom, typeof(string), null, nameof(String))]
        [DataRow(DataType.Custom, typeof(short), null, nameof(Int16))]
        [DataRow(DataType.Custom, typeof(ushort), null, nameof(UInt16))]
        [DataRow(DataType.Custom, typeof(int), null, nameof(Int32))]
        [DataRow(DataType.Custom, typeof(uint), null, nameof(UInt32))]
        [DataRow(DataType.Custom, typeof(long), null, nameof(Int64))]
        [DataRow(DataType.Custom, typeof(ulong), null, nameof(UInt64))]
        [DataRow(DataType.Custom, typeof(float), null, nameof(Single))]
        [DataRow(DataType.Custom, typeof(double), null, nameof(Double))]
        public async Task MapperShoudReturnCorrectMappingAsync(DataType dataType, Type systemType, Type valiationAttribute, string? customDataType)
        {
            Argument? argument = null;

            if (dataType == DataType.Custom)
            {
                argument = new Argument("arg1", "val1", customDataType!);
            }
            else
            {
                argument = new Argument("arg1", "val1", dataType);
            }

            var result = await mapper.MapAsync(new DataAnnotationMapperContext(argument));
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(systemType, result.MappedSystemType);
            Assert.AreEqual(valiationAttribute, result.MappedValidationAttribute);
        }

        [TestMethod]
        public async Task NullOrWhitespaceCustomDataTypeShoudErrorAsync()
        {
            Argument test = new Argument("arg1", "val1", "  ");
            var result = await mapper.MapAsync(new DataAnnotationMapperContext(test));
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument custom data type is null or whitespace. argument=arg1");

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Argument test2 = new Argument("arg2", "val2", customDataType: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            result = await mapper.MapAsync(new DataAnnotationMapperContext(test2));
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument custom data type is null or whitespace. argument=arg2");
        }

        [TestMethod]
        public async Task UnsupportedCustomDataTypeShouldErrorAsync()
        {
            var argument = new Argument("arg1", "val1", "unsupported_custom");
            var result = await mapper.MapAsync(new DataAnnotationMapperContext(argument));
            TestHelper.AssertOneImlxError(result, Errors.UnsupportedArgument, "The argument custom data type is not supported. argument=arg1 custom_data_type=unsupported_custom");
        }

        [TestMethod]
        public async Task UnsupportedDataTypeShouldErrorAsync()
        {
            var argument = new Argument("arg1", "val1", (DataType)int.MaxValue);
            var result = await mapper.MapAsync(new DataAnnotationMapperContext(argument));
            TestHelper.AssertOneImlxError(result, Errors.UnsupportedArgument, "The argument data type is not supported. argument=arg1 data_type=2147483647");
        }

        protected override void OnTestInitialize()
        {
            options = MockCliOptions.New();
            mapper = new DataAnnotationMapper(options, TestLogger.Create<DataAnnotationMapper>());
        }

        private IArgumentMapper mapper = null!;
        private CliOptions options = null!;
    }
}
