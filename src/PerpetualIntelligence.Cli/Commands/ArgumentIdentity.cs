/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// Identifies a command argument uniquely.
    /// </summary>
    public class ArgumentIdentity
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="dataType">The argument data type.</param>
        /// <param name="required">The argument is optional or required.</param>
        /// <param name="description">The argument description.</param>
        /// <param name="supportedValues">The supported values.</param>
        [ToUnitTest("null checks")]
        public ArgumentIdentity(string name, DataType dataType, bool required = false, string? description = null, object[]? supportedValues = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            }

            Name = name;
            DataType = dataType;
            Required = required;
            Description = description;
            SupportedValues = supportedValues;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="customDataType">The argument custom data type.</param>
        /// <param name="required">The argument is optional or required.</param>
        /// <param name="description">The argument description.</param>
        /// <param name="supportedValues">The supported values.</param>
        [ToUnitTest("null checks")]
        public ArgumentIdentity(string name, string customDataType, bool required = false, string? description = null, object[]? supportedValues = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            }

            Name = name;
            DataType = DataType.Custom;
            CustomDataType = customDataType;
            Required = required;
            Description = description;
            SupportedValues = supportedValues;
        }

        /// <summary>
        /// The argument custom data type.
        /// </summary>
        /// <remarks>This custom data type is used only if the <see cref="DataType"/> property is set to <see cref="DataType.Custom"/>.</remarks>
        public string? CustomDataType { get; set; }

        /// <summary>
        /// The argument data type. Defaults to <see cref="DataType.Text"/>.
        /// </summary>
        public DataType DataType { get; set; } = DataType.Text;

        /// <summary>
        /// The argument description.
        /// </summary>
        /// <remarks>The argument id is unique across all commands.</remarks>
        public string? Description { get; set; }

        /// <summary>
        /// Determines whether the argument is disabled
        /// </summary>
        public bool? Disabled { get; set; }

        /// <summary>
        /// The argument name.
        /// </summary>
        /// <remarks>The argument name is unique within a command.</remarks>
        public string Name { get; set; }

        /// <summary>
        /// Determines whether the argument is obsolete.
        /// </summary>
        public bool? Obsolete { get; set; }

        /// <summary>
        /// Determines whether the argument is required to execute the command.
        /// </summary>
        public bool? Required { get; set; }

        /// <summary>
        /// The supported values.
        /// </summary>
        public object[]? SupportedValues { get; set; }
    }
}
