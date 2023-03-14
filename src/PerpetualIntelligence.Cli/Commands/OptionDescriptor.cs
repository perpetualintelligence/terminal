/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Checkers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The <see cref="OptionDescriptor"/> class defines the command option identity, data type, and data validation
    /// behavior. We also refer to options as command options or command flags.
    /// </summary>
    /// <seealso cref="Option"/>
    /// <seealso cref="CommandString"/>
    /// <seealso cref="CommandDescriptor"/>
    public sealed class OptionDescriptor
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The option id.</param>
        /// <param name="dataType">The option data type.</param>
        /// <param name="description">The option description.</param>
        /// <param name="required">The option is required.</param>
        /// <param name="defaultValue">The option default value.</param>
        public OptionDescriptor(string id, DataType dataType, string description, bool? required = null, object? defaultValue = null)
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
        /// <param name="id">The option id.</param>
        /// <param name="customDataType">The option custom data type.</param>
        /// <param name="description">The option description.</param>
        /// <param name="required">The option is required.</param>
        /// <param name="defaultValue">The option default value.</param>
        public OptionDescriptor(string id, string customDataType, string description, bool? required = null, object? defaultValue = null)
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
        /// The option alias.
        /// </summary>
        /// <remarks>
        /// The option alias is unique within a command. Argument alias supports the legacy apps that identified a
        /// command option with an id and an alias string. For modern console apps, we recommend using just an
        /// option identifier. The core data model is optimized to work with option id. In general, an app should
        /// not identify the same option with multiple strings.
        /// </remarks>
        public string? Alias { get; set; }

        /// <summary>
        /// The option custom data type.
        /// </summary>
        /// <remarks>This custom data type is used only if the <see cref="DataType"/> property is set to <see cref="DataType.Custom"/>.</remarks>
        public string? CustomDataType { get; set; }

        /// <summary>
        /// The custom properties.
        /// </summary>
        public Dictionary<string, object>? CustomProperties { get; set; }

        /// <summary>
        /// The option data type.
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// The default option value. <c>null</c> means the option does not support a default value.
        /// </summary>
        /// <remarks>
        /// If <see cref="DefaultValue"/> is set to a non <c>null</c> value, then the option will have
        /// <see cref="DefaultValue"/>, if a user or an app does not specify any value.
        /// </remarks>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// The option description.
        /// </summary>
        /// <remarks>The option id is unique across all commands.</remarks>
        public string? Description { get; set; }

        /// <summary>
        /// Determines whether the option is disabled
        /// </summary>
        public bool? Disabled { get; set; }

        /// <summary>
        /// The option id.
        /// </summary>
        /// <remarks>The option id is unique within a command.</remarks>
        public string Id { get; set; }

        /// <summary>
        /// Determines whether the option is obsolete.
        /// </summary>
        public bool? Obsolete { get; set; }

        /// <summary>
        /// Determines is the option is required.
        /// </summary>
        public bool? Required { get; private set; }

        /// <summary>
        /// The option value checkers.
        /// </summary>
        public IEnumerable<IOptionValueChecker>? ValueCheckers
        {
            get
            {
                return valueCheckers;
            }

            set
            {
                valueCheckers = value;
                SetValidationRequired();
            }
        }

        /// <summary>
        /// Sets the option as required.
        /// </summary>
        private void SetValidationRequired()
        {
            if (valueCheckers != null)
            {
                if (valueCheckers.Any(e => e.GetRawType() == typeof(RequiredAttribute)))
                {
                    Required = true;
                }
            }
        }

        private IEnumerable<IOptionValueChecker>? valueCheckers;
    }
}