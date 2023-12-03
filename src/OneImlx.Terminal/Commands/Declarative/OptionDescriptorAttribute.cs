/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Commands.Declarative
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
        /// <param name="flags">The option flags.</param>
        /// <param name="alias">The option alias.</param>
        public OptionDescriptorAttribute(string id, string dataType, string description, OptionFlags flags, string? alias = null)
        {
            Id = id;
            DataType = dataType;
            Description = description;
            Flags = flags;
            Alias = alias;
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
        public string? Alias { get; }

        /// <summary>
        /// The option data type.
        /// </summary>
        public string DataType { get; }

        /// <summary>
        /// The option description.
        /// </summary>
        /// <remarks>The option id is unique across all commands.</remarks>
        public string Description { get; }

        /// <summary>
        /// The option id.
        /// </summary>
        /// <remarks>The option id is unique within a command.</remarks>
        public string Id { get; }

        /// <summary>
        /// The option flags.
        /// </summary>
        public OptionFlags Flags { get; }
    }
}