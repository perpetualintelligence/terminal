/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;
using System.Collections.Generic;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// Describes a <see cref="Command"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="CommandDescriptor"/> defines <see cref="Command"/> identity and its supported
    /// <see cref="Argument"/> . The <see cref="Command"/> is a runtime validated representation of an actual command
    /// and its argument values passed by a user or an application.
    /// </remarks>
    /// <seealso cref="Command"/>
    /// <seealso cref="ArgumentDescriptor"/>
    public sealed class CommandDescriptor
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The command id.</param>
        /// <param name="name">The command name.</param>
        /// <param name="prefix">The command prefix to map the command string.</param>
        /// <param name="argumentDescriptors">The command argument descriptors.</param>
        /// <param name="description">The command description.</param>
        /// <param name="checker">The command checker.</param>
        /// <param name="runner">The command runner.</param>
        /// <param name="properties">The custom properties.</param>
        public CommandDescriptor(string id, string name, string prefix, ArgumentDescriptors? argumentDescriptors = null, string? description = null, Type? checker = null, Type? runner = null, Dictionary<string, object>? properties = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            }

            if (string.IsNullOrEmpty(prefix))
            {
                throw new ArgumentException($"'{nameof(prefix)}' cannot be null or empty.", nameof(prefix));
            }

            Id = id;
            Name = name;
            Prefix = prefix;
            Description = description;
            ArgumentDescriptors = argumentDescriptors;
            Checker = checker;
            Runner = runner;
            Properties = properties;
        }

        /// <summary>
        /// The command arguments identity.
        /// </summary>
        public ArgumentDescriptors? ArgumentDescriptors { get; set; }

        /// <summary>
        /// The command checker.
        /// </summary>
        public Type? Checker { get; set; }

        /// <summary>
        /// The command description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The command id.
        /// </summary>
        /// <remarks>The command id is unique across all command group.</remarks>
        public string Id { get; set; }

        /// <summary>
        /// Determines if identity represents a command group.
        /// </summary>
        public bool IsGroup { get; set; }

        /// <summary>
        /// The command name.
        /// </summary>
        /// <remarks>The command name is unique within a command group.</remarks>
        public string Name { get; set; }

        /// <summary>
        /// The prefix to match the command string.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// The custom properties.
        /// </summary>
        public Dictionary<string, object>? Properties { get; set; }

        /// <summary>
        /// The command runner.
        /// </summary>
        public Type? Runner { get; set; }

        /// <summary>
        /// Attempts to find an argument descriptor.
        /// </summary>
        /// <param name="id">The argument descriptor identifier.</param>
        /// <param name="argumentDescriptor">The argument descriptor if found.</param>
        /// <returns><c>true</c> if an argument descriptor exist in the collection, otherwise <c>false</c>.</returns>
        public bool TryGetArgumentDescriptor(string id, out ArgumentDescriptor argumentDescriptor)
        {
            if (ArgumentDescriptors == null)
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                argumentDescriptor = default;
                return false;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }

#if NETSTANDARD2_1_OR_GREATER
            return ArgumentDescriptors.TryGetValue(id, out argumentDescriptor);
#else
            if (ArgumentDescriptors.Contains(id))
            {
                argumentDescriptor = ArgumentDescriptors[id];
                return true;
            }
            else
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                argumentDescriptor = default;
                return false;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
#endif
        }
    }
}
