/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
        /// <param name="required">The argument is required.</param>
        /// <param name="description">The argument description.</param>
        /// <param name="validationAttributes">The data validation attributes.</param>
        [ToUnitTest("null checks")]
        public ArgumentIdentity(string name, DataType dataType, bool required = false, string? description = null, ValidationAttribute[]? validationAttributes = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            }

            Name = name;
            DataType = dataType;
            Description = description;

            if (validationAttributes != null)
            {
                ValidationAttributes = new HashSet<ValidationAttribute>(validationAttributes);
            }

            if (required)
            {
                SetRequired();
            }
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="customDataType">The argument custom data type.</param>
        /// <param name="required">The argument is required.</param>
        /// <param name="description">The argument description.</param>
        /// <param name="validationAttributes">The data validation attributes.</param>
        [ToUnitTest("null checks")]
        public ArgumentIdentity(string name, string customDataType, bool required = false, string? description = null, ValidationAttribute[]? validationAttributes = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            }

            Name = name;
            DataType = DataType.Custom;
            CustomDataType = customDataType;
            Description = description;

            if (validationAttributes != null)
            {
                ValidationAttributes = new HashSet<ValidationAttribute>(validationAttributes);
            }

            if (required)
            {
                SetRequired();
            }
        }

        /// <summary>
        /// The argument custom data type.
        /// </summary>
        /// <remarks>This custom data type is used only if the <see cref="DataType"/> property is set to <see cref="DataType.Custom"/>.</remarks>
        public string? CustomDataType { get; set; }

        /// <summary>
        /// The argument data type.
        /// </summary>
        public DataType DataType { get; set; }

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
        /// Determines is the argument is required.
        /// </summary>
        public bool IsRequired
        {
            get
            {
                if (ValidationAttributes == null)
                {
                    return false;
                }

                return ValidationAttributes.Contains(new RequiredAttribute());
            }
        }

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
        /// The data annotation validation attributes to check the argument value.
        /// </summary>
        public IEnumerable<ValidationAttribute>? ValidationAttributes { get; set; }

        /// <summary>
        /// Sets the argument as required.
        /// </summary>
        protected void SetRequired()
        {
            if (ValidationAttributes == null)
            {
                ValidationAttributes = new HashSet<ValidationAttribute>() { new RequiredAttribute() };
            }
            else
            {
                List<ValidationAttribute> attributes = new(ValidationAttributes);
                attributes.Add(new RequiredAttribute());
                ValidationAttributes = attributes;
            }
        }
    }
}
