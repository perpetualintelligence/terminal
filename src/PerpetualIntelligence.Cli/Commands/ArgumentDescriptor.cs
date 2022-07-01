/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The <see cref="ArgumentDescriptor"/> class defines the command argument identity, data type, and data validation
    /// behavior. We also refer to arguments as command options or command flags.
    /// </summary>
    /// <seealso cref="Argument"/>
    /// <seealso cref="CommandString"/>
    /// <seealso cref="CommandDescriptor"/>
    public sealed class ArgumentDescriptor
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The argument id.</param>
        /// <param name="dataType">The argument data type.</param>
        /// <param name="description">The argument description.</param>
        /// <param name="required">The argument is required.</param>
        /// <param name="defaultValue">The argument default value.</param>
        public ArgumentDescriptor(string id, DataType dataType, string description, bool? required = null, object? defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
            }

            Id = id;
            DataType = dataType;
            Description = description;
            DefaultValue = defaultValue;
            Required = required;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The argument id.</param>
        /// <param name="customDataType">The argument custom data type.</param>
        /// <param name="description">The argument description.</param>
        /// <param name="required">The argument is required.</param>
        /// <param name="defaultValue">The argument default value.</param>
        public ArgumentDescriptor(string id, string customDataType, string description, bool? required = null, object? defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
            }

            Id = id;
            DataType = DataType.Custom;
            CustomDataType = customDataType;
            Description = description;
            DefaultValue = defaultValue;
            Required = required;
        }

        /// <summary>
        /// The argument alias.
        /// </summary>
        /// <remarks>
        /// The argument alias is unique within a command. Argument alias supports the legacy apps that identified a
        /// command argument with an id and an alias string. For modern console apps, we recommend using just an
        /// argument identifier. The core data model is optimized to work with argument id. In general, an app should
        /// not identify the same argument with multiple strings.
        /// </remarks>
        public string? Alias { get; set; }

        /// <summary>
        /// The argument custom data type.
        /// </summary>
        /// <remarks>This custom data type is used only if the <see cref="DataType"/> property is set to <see cref="DataType.Custom"/>.</remarks>
        public string? CustomDataType { get; set; }

        /// <summary>
        /// The custom properties.
        /// </summary>
        public Dictionary<string, object>? CustomProperties { get; set; }

        /// <summary>
        /// The argument data type.
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// The default argument value. <c>null</c> means the argument does not support a default value.
        /// </summary>
        /// <remarks>
        /// If <see cref="DefaultValue"/> is set to a non <c>null</c> value, then the argument will have
        /// <see cref="DefaultValue"/>, if a user or an app does not specify any value.
        /// </remarks>
        public object? DefaultValue { get; set; }

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
        /// The argument id.
        /// </summary>
        /// <remarks>The argument id is unique within a command.</remarks>
        public string Id { get; set; }

        /// <summary>
        /// Determines whether the argument is obsolete.
        /// </summary>
        public bool? Obsolete { get; set; }

        /// <summary>
        /// Determines is the argument is required.
        /// </summary>
        public bool? Required { get; set; }

        /// <summary>
        /// The data annotation validation attributes to check the argument value.
        /// </summary>
        public IEnumerable<ValidationAttribute>? ValidationAttributes
        {
            get
            {
                return validationAttributes;
            }

            set
            {
                validationAttributes = value;
                SetValidationRequired();
            }
        }

        /// <summary>
        /// Sets the argument as required.
        /// </summary>
        private void SetValidationRequired()
        {
            if (validationAttributes != null)
            {
                if (validationAttributes.Contains(new RequiredAttribute()))
                {
                    Required = true;
                }
            }
        }

        private IEnumerable<ValidationAttribute>? validationAttributes;
    }
}
