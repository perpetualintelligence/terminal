/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;

using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Mappers
{
    [TestClass]
    public class DataAnnotationMapperTests : InitializerTests
    {
        public DataAnnotationMapperTests() : base(TestLogger.Create<DataAnnotationMapperTests>())
        {
        }

        [DataTestMethod]
        [DataRow("invalid")]
        [DataRow("@[]asdas")]
        [DataRow("12343")]
        public async Task MapperShouldThrowForInvalidDataType(string dataType)
        {
            Option option = new(new OptionDescriptor("opt1", dataType, "desc", OptionFlags.None), "val1");
            Func<Task> result = async () => await mapper.MapToTypeAsync(new DataTypeMapperContext<Option>(option));
            await result.Should().ThrowAsync<TerminalException>().WithMessage($"The option data type is not supported. option=opt1 data_type={dataType}");
        }

        [DataTestMethod]
        [DataRow(nameof(Boolean), typeof(bool))]
        [DataRow(nameof(String), typeof(string))]
        [DataRow(nameof(Int16), typeof(short))]
        [DataRow(nameof(UInt16), typeof(ushort))]
        [DataRow(nameof(Int32), typeof(int))]
        [DataRow(nameof(UInt32), typeof(uint))]
        [DataRow(nameof(Int64), typeof(long))]
        [DataRow(nameof(UInt64), typeof(ulong))]
        [DataRow(nameof(Single), typeof(float))]
        [DataRow(nameof(Double), typeof(double))]
        [DataRow(nameof(DateTime), typeof(DateTime))]
        public async Task MapperShouldReturnCorrectMappingAsync(string dataType, Type systemType)
        {
            Option option = new(new OptionDescriptor("opt1", dataType, "desc", OptionFlags.None), "val1");
            var result = await mapper.MapToTypeAsync(new DataTypeMapperContext<Option>(option));
            Assert.AreEqual(systemType, result.MappedType);
        }

        [TestMethod]
        public async Task NullOrWhitespaceDataTypeShouldErrorAsync()
        {
            Option test = new(new OptionDescriptor("opt1", "   ", "desc", OptionFlags.None), "val1");
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => mapper.MapToTypeAsync(new DataTypeMapperContext<Option>(test)), TerminalErrors.InvalidOption, "The option data type cannot be null or whitespace. option=opt1");

            test = new(new OptionDescriptor("opt1", "", "desc", OptionFlags.None), "val1");
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => mapper.MapToTypeAsync(new DataTypeMapperContext<Option>(test)), TerminalErrors.InvalidOption, "The option data type cannot be null or whitespace. option=opt1");

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            test = new(new OptionDescriptor("opt1", null, "desc", OptionFlags.None), "val1");
            await TestHelper.AssertThrowsErrorExceptionAsync<TerminalException>(() => mapper.MapToTypeAsync(new DataTypeMapperContext<Option>(test)), TerminalErrors.InvalidOption, "The option data type cannot be null or whitespace. option=opt1");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        protected override void OnTestInitialize()
        {
            options = MockTerminalOptions.NewLegacyOptions();
            mapper = new DataTypeMapper<Option>(options, TestLogger.Create<DataTypeMapper<Option>>());
        }

        private IDataTypeMapper<Option> mapper = null!;
        private TerminalOptions options = null!;
    }
}