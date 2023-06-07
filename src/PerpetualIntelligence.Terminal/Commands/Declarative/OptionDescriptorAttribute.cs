/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Terminal.Commands.Declarative
{
    /// <summary>
    /// Declares an <see cref="OptionDescriptor"/> for a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class OptionDescriptorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The option id.</param>
        /// <param name="dataType">The option data type.</param>
        /// <param name="description">The option description.</param>
        public OptionDescriptorAttribute(string id, DataType dataType, string description)
        {
            Id = id;
            DataType = dataType;
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The option id.</param>
        /// <param name="customDataType">The option custom data type.</param>
        /// <param name="description">The option description.</param>
        public OptionDescriptorAttribute(string id, string customDataType, string description)
        {
            Id = id;
            CustomDataType = customDataType;
            DataType = DataType.Custom;
            Description = description;
        }

        /// <summary>
        /// The option alias.
        /// </summary>
        /// <remarks>
        /// The option alias is unique within a command. Option alias supports the legacy apps that identified a
        /// command option with an id and an alias string. For modern console apps, we recommend using just an
        /// option identifier. The core data model is optimized to work with option id. In general, an app should
        /// not identify the same option with multiple strings.
        /// </remarks>
        public string? Alias { get; set; }

        /// <summary>
        /// The option custom data type.
        /// </summary>
        /// <remarks>This custom data type is used only if the <see cref="DataType"/> property is set to <see cref="DataType.Custom"/>.</remarks>
        public string? CustomDataType { get; }

        /// <summary>
        /// The option data type.
        /// </summary>
        public DataType DataType { get; }

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
        public string Description { get; }

        /// <summary>
        /// Determines whether the option is disabled
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// The option id.
        /// </summary>
        /// <remarks>The option id is unique within a command.</remarks>
        public string Id { get; }

        /// <summary>
        /// Determines whether the option is obsolete.
        /// </summary>
        public bool Obsolete { get; set; }

        /// <summary>
        /// Determines is the option is required.
        /// </summary>
        public bool Required { get; set; }
    }
}
