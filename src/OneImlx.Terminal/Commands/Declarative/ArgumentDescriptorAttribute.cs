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
    public sealed class ArgumentDescriptorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="order">The argument order.</param>
        /// <param name="id">The argument id.</param>
        /// <param name="dataType">The argument data type.</param>
        /// <param name="description">The argument description.</param>
        /// <param name="flags">The argument flags.</param>
        public ArgumentDescriptorAttribute(int order, string id, string dataType, string description, ArgumentFlags flags)
        {
            Order = order;
            Id = id;
            DataType = dataType;
            Description = description;
            Flags = flags;
        }

        /// <summary>
        /// The argument data type.
        /// </summary>
        public string DataType { get; }

        /// <summary>
        /// The argument description.
        /// </summary>
        /// <remarks>The argument id is unique across all commands.</remarks>
        public string Description { get; }

        /// <summary>
        /// The argument order.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// The argument id.
        /// </summary>
        /// <remarks>The argument id is unique within a command.</remarks>
        public string Id { get; }

        /// <summary>
        /// The argument flags.
        /// </summary>
        public ArgumentFlags Flags { get; }
    }
}