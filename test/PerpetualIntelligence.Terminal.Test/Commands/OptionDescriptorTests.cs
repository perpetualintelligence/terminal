/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Terminal.Commands.Checkers;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace PerpetualIntelligence.Terminal.Commands
{
    public class OptionDescriptorTests
    {
        [Fact]
        public void CustomDataTypeShouldSetDataType()
        {
            OptionDescriptor arg = new("name", "custom", "test desc", OptionFlags.Required);
            arg.DataType.Should().Be("custom");
        }

        [Fact]
        public void MultipleFlagsShouldBeSet()
        {
            OptionDescriptor arg = new("name", "custom", "test desc", OptionFlags.Required | OptionFlags.Obsolete | OptionFlags.Disabled);
            arg.Flags.Should().Be(OptionFlags.Required | OptionFlags.Obsolete | OptionFlags.Disabled);
        }

        [Fact]
        public void RequiredExplicitlySetShouldNotSetDataAnnotationRequiredAttribute()
        {
            OptionDescriptor arg = new("name", "custom", "test desc", OptionFlags.Required);
            arg.ValueCheckers.Should().BeNull();
            arg.Flags.Should().Be(OptionFlags.Required);
        }

        [Fact]
        public void RequiredShouldBeSetWithDataAnnotationRequiredAttribute()
        {
            OptionDescriptor arg = new("name", "custom", "test desc", OptionFlags.None) { ValueCheckers = new[] { new DataValidationOptionValueChecker(new RequiredAttribute()) } };
            arg.ValueCheckers.Should().NotBeNull();
            arg.Flags.Should().Be(OptionFlags.Required);
        }
    }
}