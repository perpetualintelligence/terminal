/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Test.FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Mappers
{
    public class DataAnnotationMapperTests
    {
        public DataAnnotationMapperTests()
        {
            options = MockTerminalOptions.NewLegacyOptions();
            mapper = new DataTypeMapper<Option>(options, new LoggerFactory().CreateLogger<DataTypeMapper<Option>>());
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("@[]asdas")]
        [InlineData("12343")]
        public async Task MapperShouldThrowForInvalidDataType(string dataType)
        {
            Option option = new(new OptionDescriptor("opt1", dataType, "desc", OptionFlags.None), "val1");
            Func<Task> result = async () => await mapper.MapToTypeAsync(new DataTypeMapperContext<Option>(option));
            await result.Should().ThrowAsync<TerminalException>().WithMessage($"The option data type is not supported. option=opt1 data_type={dataType}");
        }

        [Theory]
        [InlineData(nameof(Boolean), typeof(bool))]
        [InlineData(nameof(String), typeof(string))]
        [InlineData(nameof(Int16), typeof(short))]
        [InlineData(nameof(UInt16), typeof(ushort))]
        [InlineData(nameof(Int32), typeof(int))]
        [InlineData(nameof(UInt32), typeof(uint))]
        [InlineData(nameof(Int64), typeof(long))]
        [InlineData(nameof(UInt64), typeof(ulong))]
        [InlineData(nameof(Single), typeof(float))]
        [InlineData(nameof(Double), typeof(double))]
        [InlineData(nameof(DateTime), typeof(DateTime))]
        public async Task MapperShouldReturnCorrectMappingAsync(string dataType, Type systemType)
        {
            Option option = new(new OptionDescriptor("opt1", dataType, "desc", OptionFlags.None), "val1");
            var result = await mapper.MapToTypeAsync(new DataTypeMapperContext<Option>(option));
            result.MappedType.Should().Be(systemType);
        }

        [Fact]
        public async Task NullOrWhitespaceDataTypeShouldErrorAsync()
        {
            Option test = new(new OptionDescriptor("opt1", "   ", "desc", OptionFlags.None), "val1");
            Func<Task> func = async () => await mapper.MapToTypeAsync(new DataTypeMapperContext<Option>(test));
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidOption).WithErrorDescription("The option data type cannot be null or whitespace. option=opt1");

            test = new(new OptionDescriptor("opt1", "", "desc", OptionFlags.None), "val1");
            func = async () => await mapper.MapToTypeAsync(new DataTypeMapperContext<Option>(test));
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidOption).WithErrorDescription("The option data type cannot be null or whitespace. option=opt1");

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            test = new(new OptionDescriptor("opt1", null, "desc", OptionFlags.None), "val1");
            func = async () => await mapper.MapToTypeAsync(new DataTypeMapperContext<Option>(test));
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidOption).WithErrorDescription("The option data type cannot be null or whitespace. option=opt1");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        private IDataTypeMapper<Option> mapper = null!;
        private TerminalOptions options = null!;
    }
}