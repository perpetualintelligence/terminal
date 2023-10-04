/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// The <see cref="CommandDescriptor"/> defines the command identity and its supported options that an end-user or
    /// an application can use. You can also describe the command behavior, such as whether the command is a root,
    /// grouped, or subcommand.
    /// </summary>
    /// <seealso cref="Command"/>
    /// <seealso cref="OptionDescriptor"/>
    /// <seealso cref="ArgumentDescriptor"/>
    public sealed class CommandDescriptor
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The command id.</param>
        /// <param name="name">The command name.</param>
        /// <param name="description">The command description.</param>
        /// <param name="type">The command type.</param>
        /// <param name="flags">The command flags.</param>
        /// <param name="owners">The command owners.</param>
        /// <param name="argumentDescriptors">The argument descriptors.</param>
        /// <param name="optionDescriptors">The option descriptors.</param>
        /// <param name="tagIds">The tag identifiers.</param>
        /// <param name="customProperties">The custom properties.</param>
        public CommandDescriptor(
            string id,
            string name,
            string description,
            CommandType type,
            CommandFlags flags,
            OwnerIdCollection? owners = null,
            ArgumentDescriptors? argumentDescriptors = null,
            OptionDescriptors? optionDescriptors = null,
            TagIdCollection? tagIds = null,
            Dictionary<string, object>? customProperties = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            }

            Id = id;
            Name = name;
            Description = description;
            Type = type;
            Flags = flags;
            OwnerIds = owners;
            OptionDescriptors = optionDescriptors;
            TagIds = tagIds;
            CustomProperties = customProperties;
            ArgumentDescriptors = argumentDescriptors;
        }

        /// <summary>
        /// The command option descriptors.
        /// </summary>
        public OptionDescriptors? OptionDescriptors { get; internal set; }

        /// <summary>
        /// The command argument descriptors.
        /// </summary>
        public ArgumentDescriptors? ArgumentDescriptors { get; internal set; }

        /// <summary>
        /// The command checker.
        /// </summary>
        public Type? Checker { get; internal set; }

        /// <summary>
        /// The custom properties.
        /// </summary>
        public Dictionary<string, object>? CustomProperties { get; internal set; }

        /// <summary>
        /// The command description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The command type.
        /// </summary>
        public CommandType Type { get; }

        /// <summary>
        /// The command flags.
        /// </summary>
        public CommandFlags Flags { get; }

        /// <summary>
        /// The command owner identifiers.
        /// </summary>
        /// <remarks>
        /// The root command will not have any owner.
        /// </remarks>
        public OwnerIdCollection? OwnerIds { get; internal set; }

        /// <summary>
        /// The command id.
        /// </summary>
        /// <remarks>The command id is unique across all commands within a group or a root.</remarks>
        public string Id { get; }

        /// <summary>
        /// The command display name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The command runner.
        /// </summary>
        public Type? Runner { get; internal set; }

        /// <summary>
        /// The tags to find the command.
        /// </summary>
        public TagIdCollection? TagIds { get; internal set; }
    }
}