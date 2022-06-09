/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Extensions;
using PerpetualIntelligence.Shared.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PerpetualIntelligence.Cli.Test")]

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The <see cref="CommandDescriptor"/> defines the command identity and its supported arguments that an end-user or
    /// an application can use. You can also describe the command behavior, such as whether the command is a root,
    /// grouped, or subcommand.
    /// </summary>
    /// <seealso cref="Command"/>
    /// <seealso cref="ArgumentDescriptor"/>
    /// <seealso cref="ICliBuilderExtensions.AddDescriptor{TRunner, TChecker}(Integration.ICliBuilder, CommandDescriptor, bool, bool, bool)"/>
    public sealed class CommandDescriptor
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The command id.</param>
        /// <param name="name">The command name.</param>
        /// <param name="prefix">The command prefix to map the command string.</param>
        /// <param name="description">The command description.</param>
        /// <param name="argumentDescriptors">The command argument descriptors.</param>
        /// <param name="properties">The custom properties.</param>
        /// <param name="defaultArgument">The default argument.</param>
        public CommandDescriptor(string id, string name, string prefix, string description, ArgumentDescriptors? argumentDescriptors = null, Dictionary<string, object>? properties = null, string? defaultArgument = null)
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
            Properties = properties;
            DefaultArgument = defaultArgument;
        }

        /// <summary>
        /// The command argument descriptors.
        /// </summary>
        public ArgumentDescriptors? ArgumentDescriptors { get; }

        /// <summary>
        /// The default argument. <c>null</c> means the command does not support a default argument.
        /// </summary>
        /// <remarks>
        /// <see cref="DefaultArgument"/> is not the default argument value (see
        /// <see cref="ArgumentDescriptor.DefaultValue"/>), it is the default argument identifier (see
        /// <see cref="ArgumentDescriptor.Id"/>) whose value is populated automatically based on the
        /// <see cref="CommandString"/>. If <see cref="DefaultArgument"/> is set to a non <c>null</c> value, then the
        /// <see cref="ICommandExtractor"/> will attempt to extract the value from the <see cref="CommandString"/> and
        /// put it in an <see cref="Argument"/> identified by <see cref="DefaultArgument"/>.
        /// </remarks>
        /// <seealso cref="ArgumentDescriptor.DefaultValue"/>
        public string? DefaultArgument { get; }

        /// <summary>
        /// The command description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The command id.
        /// </summary>
        /// <remarks>The command id is unique across all commands.</remarks>
        public string Id { get; }

        /// <summary>
        /// Returns <c>true</c> if this descriptor represents a grouped command; otherwise, <c>false</c>.
        /// </summary>
        public bool IsGroup => _isGroup;

        /// <summary>
        /// Returns <c>true</c> if this descriptor represents a protected command; otherwise, <c>false</c>.
        /// </summary>
        public bool IsProtected => _isProtected;

        /// <summary>
        /// Returns <c>true</c> if this descriptor represents a root command; otherwise, <c>false</c>.
        /// </summary>
        public bool IsRoot => _isRoot;

        /// <summary>
        /// The command name.
        /// </summary>
        /// <remarks>The command name is unique within a grouped command.</remarks>
        public string Name { get; }

        /// <summary>
        /// The prefix to match the command string.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// The custom properties.
        /// </summary>
        public Dictionary<string, object>? Properties { get; }

        /// <summary>
        /// Attempts to find an argument descriptor.
        /// </summary>
        /// <param name="argId">The argument descriptor identifier.</param>
        /// <param name="argumentDescriptor">The argument descriptor if found.</param>
        /// <returns><c>true</c> if an argument descriptor exist in the collection, otherwise <c>false</c>.</returns>
        public bool TryGetArgumentDescriptor(string argId, out ArgumentDescriptor argumentDescriptor)
        {
            if (ArgumentDescriptors == null)
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                argumentDescriptor = default;
                return false;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }

#if NETSTANDARD2_1_OR_GREATER
            return ArgumentDescriptors.TryGetValue(argId, out argumentDescriptor);
#else
            if (ArgumentDescriptors.Contains(argId))
            {
                argumentDescriptor = ArgumentDescriptors[argId];
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

        [InternalInfrastructure]
        internal Type? _checker { get; set; }

        [InternalInfrastructure]
        internal bool _isGroup { get; set; }

        [InternalInfrastructure]
        internal bool _isProtected { get; set; }

        [InternalInfrastructure]
        internal bool _isRoot { get; set; }

        [InternalInfrastructure]
        internal Type? _runner { get; set; }
    }
}
