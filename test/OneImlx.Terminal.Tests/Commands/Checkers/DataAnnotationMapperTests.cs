/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Shared;
using OneImlx.Test.FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Checkers
{
    public class DataAnnotationMapperTests
    {
        public DataAnnotationMapperTests()
        {
            options = MockTerminalOptions.NewLegacyOptions();
            mapper = new DataTypeMapper<Option>(options, new LoggerFactory().CreateLogger<DataTypeMapper<Option>>());
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
            var result = await mapper.MapToTypeAsync(option);
            result.MappedType.Should().Be(systemType);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("@[]asdas")]
        [InlineData("12343")]
        public async Task MapperShouldThrowForInvalidDataType(string dataType)
        {
            Option option = new(new OptionDescriptor("opt1", dataType, "desc", OptionFlags.None), "val1");
            Func<Task> result = async () => await mapper.MapToTypeAsync(option);
            await result.Should().ThrowAsync<TerminalException>().WithMessage($"The value data type is not supported. value=opt1 data_type={dataType}");
        }

        [Fact]
        public async Task NullOrWhitespaceDataTypeShouldErrorAsync()
        {
            Option test = new(new OptionDescriptor("opt1", "   ", "desc", OptionFlags.None), "val1");
            Func<Task> func = async () => await mapper.MapToTypeAsync(test);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidRequest).WithErrorDescription("The value data type cannot be null or whitespace. value=opt1");

            test = new(new OptionDescriptor("opt1", "", "desc", OptionFlags.None), "val1");
            func = async () => await mapper.MapToTypeAsync(test);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidRequest).WithErrorDescription("The value data type cannot be null or whitespace. value=opt1");

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            test = new(new OptionDescriptor("opt1", null, "desc", OptionFlags.None), "val1");
            func = async () => await mapper.MapToTypeAsync(test);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.InvalidRequest).WithErrorDescription("The value data type cannot be null or whitespace. value=opt1");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        private readonly IDataTypeMapper<Option> mapper = null!;
        private readonly TerminalOptions options = null!;
    }
}
