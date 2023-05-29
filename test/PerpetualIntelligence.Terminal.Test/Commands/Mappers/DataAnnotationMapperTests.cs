/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;

using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Mappers
{
    [TestClass]
    public class DataAnnotationMapperTests : InitializerTests
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
        public async Task MapperShouldReturnCorrectMappingAsync(DataType dataType, Type systemType, Type valiationAttribute, string? customDataType)
        {
            Option? option = null;

            if (dataType == DataType.Custom)
            {
                option = new Option(new OptionDescriptor("arg1", customDataType!, "desc"), "val1");
            }
            else
            {
                option = new Option(new OptionDescriptor("arg1", dataType, "desc"), "val1");
            }

            var result = await mapper.MapAsync(new OptionDataTypeMapperContext(option));
            Assert.AreEqual(systemType, result.MappedType);
        }

        [TestMethod]
        public async Task NullOrWhitespaceCustomDataTypeShouldErrorAsync()
        {
            Option test = new Option(new OptionDescriptor("arg1", "  ", "desc"), "val1");
            await TestHelper.AssertThrowsErrorExceptionAsync(() => mapper.MapAsync(new OptionDataTypeMapperContext(test)), Errors.InvalidOption, "The option custom data type is null or whitespace. option=arg1");

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Option test2 = new Option(new OptionDescriptor("arg2", customDataType: null, "desc"), "val2");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            await TestHelper.AssertThrowsErrorExceptionAsync(() => mapper.MapAsync(new OptionDataTypeMapperContext(test2)), Errors.InvalidOption, "The option custom data type is null or whitespace. option=arg2");
        }

        [TestMethod]
        public async Task UnsupportedCustomDataTypeShouldErrorAsync()
        {
            var option = new Option(new OptionDescriptor("arg1", "unsupported_custom", "desc"), "val1");
            await TestHelper.AssertThrowsErrorExceptionAsync(() => mapper.MapAsync(new OptionDataTypeMapperContext(option)), Errors.UnsupportedOption, "The option custom data type is not supported. option=arg1 custom_data_type=unsupported_custom");
        }

        [TestMethod]
        public async Task UnsupportedDataTypeShouldErrorAsync()
        {
            var option = new Option(new OptionDescriptor("arg1", (DataType)int.MaxValue, "desc"), "val1");
            await TestHelper.AssertThrowsErrorExceptionAsync(() => mapper.MapAsync(new OptionDataTypeMapperContext(option)), Errors.UnsupportedOption, "The option data type is not supported. option=arg1 data_type=2147483647");
        }

        protected override void OnTestInitialize()
        {
            options = MockCliOptions.New();
            mapper = new DataAnnotationsOptionDataTypeMapper(options, TestLogger.Create<DataAnnotationsOptionDataTypeMapper>());
        }

        private IOptionDataTypeMapper mapper = null!;
        private TerminalOptions options = null!;
    }
}