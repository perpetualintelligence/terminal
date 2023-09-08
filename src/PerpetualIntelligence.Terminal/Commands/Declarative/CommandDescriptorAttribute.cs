/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Commands.Declarative
{
    /// <summary>
    /// Declares a <see cref="CommandDescriptor"/> for a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CommandDescriptorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The command id.</param>
        /// <param name="name">The command name.</param>
        /// <param name="description">The command description.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="commandFlags">The command flags.</param>
        public CommandDescriptorAttribute(string id, string name, string description, CommandType commandType, CommandFlags commandFlags)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentException($"'{nameof(description)}' cannot be null or empty.", nameof(description));
            }

            Id = id;
            Name = name;
            Description = description;
            CommandType = commandType;
            CommandFlags = commandFlags;
        }

        /// <summary>
        /// The command description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The command type.
        /// </summary>
        public CommandType CommandType { get; }

        /// <summary>
        /// The command flags.
        /// </summary>
        public CommandFlags CommandFlags { get; }

        /// <summary>
        /// The command id.
        /// </summary>
        /// <remarks>The command id is unique across all commands.</remarks>
        public string Id { get; }

        /// <summary>
        /// The command name.
        /// </summary>
        /// <remarks>The command name is unique within a grouped command.</remarks>
        public string Name { get; }
    }
}