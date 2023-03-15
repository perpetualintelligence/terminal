/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Commands.Extractors;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PerpetualIntelligence.Cli.Test")]

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The <see cref="CommandDescriptor"/> defines the command identity and its supported options that an end-user or
    /// an application can use. You can also describe the command behavior, such as whether the command is a root,
    /// grouped, or subcommand.
    /// </summary>
    /// <seealso cref="Command"/>
    /// <seealso cref="OptionDescriptor"/>
    public sealed class CommandDescriptor
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The command id.</param>
        /// <param name="name">The command name.</param>
        /// <param name="prefix">The command prefix to map the command string.</param>
        /// <param name="description">The command description.</param>
        /// <param name="optionDescriptors">The option descriptors.</param>
        /// <param name="customProperties">The custom properties.</param>
        /// <param name="defaultOption">The default option.</param>
        /// <param name="tags">The tags to find a command.</param>
        public CommandDescriptor(string id, string name, string prefix, string description, OptionDescriptors? optionDescriptors = null, Dictionary<string, object>? customProperties = null, string? defaultOption = null, string[]? tags = null)
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
            OptionDescriptors = optionDescriptors;
            CustomProperties = customProperties;
            DefaultOption = defaultOption;
            Tags = tags;
        }

        /// <summary>
        /// The command option descriptors.
        /// </summary>
        public OptionDescriptors? OptionDescriptors { get; internal set; }

        /// <summary>
        /// The command checker.
        /// </summary>
        public Type? Checker { get; internal set; }

        /// <summary>
        /// The custom properties.
        /// </summary>
        public Dictionary<string, object>? CustomProperties { get; internal set; }

        /// <summary>
        /// The default option. <c>null</c> means the command does not support a default option.
        /// </summary>
        /// <remarks>
        /// <see cref="DefaultOption"/> is not the default option value (see
        /// <see cref="OptionDescriptor.DefaultValue"/>), it is the default option identifier (see
        /// <see cref="OptionDescriptor.Id"/>) whose value is populated automatically based on the
        /// <see cref="CommandString"/>. If <see cref="DefaultOption"/> is set to a non <c>null</c> value, then the
        /// <see cref="ICommandExtractor"/> will attempt to extract the value from the <see cref="CommandString"/> and
        /// put it in an <see cref="Option"/> identified by <see cref="DefaultOption"/>.
        /// </remarks>
        /// <seealso cref="OptionDescriptor.DefaultValue"/>
        public string? DefaultOption { get; internal set; }

        /// <summary>
        /// The command description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The command usage.
        /// </summary>
        public string? Usage { get; }

        /// <summary>
        /// The command id.
        /// </summary>
        /// <remarks>The command id is unique across all commands.</remarks>
        public string Id { get; }

        /// <summary>
        /// Returns <c>true</c> if this descriptor represents a grouped command; otherwise, <c>false</c>.
        /// </summary>
        public bool IsGroup { get; internal set; }

        /// <summary>
        /// Returns <c>true</c> if this descriptor represents a protected command; otherwise, <c>false</c>.
        /// </summary>
        public bool IsProtected { get; internal set; }

        /// <summary>
        /// Returns <c>true</c> if this descriptor represents a root command; otherwise, <c>false</c>.
        /// </summary>
        public bool IsRoot { get; internal set; }

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
        /// The command runner.
        /// </summary>
        public Type? Runner { get; internal set; }

        /// <summary>
        /// The tags to find the command.
        /// </summary>
        public string[]? Tags { get; internal set; }

        /// <summary>
        /// Attempts to find an option descriptor.
        /// </summary>
        /// <param name="argId">The option descriptor identifier.</param>
        /// <param name="optionDescriptor">The option descriptor if found.</param>
        /// <returns><c>true</c> if an option descriptor exist in the collection, otherwise <c>false</c>.</returns>
        public bool TryGetOptionDescriptor(string argId, out OptionDescriptor optionDescriptor)
        {
            if (OptionDescriptors == null)
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                optionDescriptor = default;
                return false;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }

#if NETSTANDARD2_1_OR_GREATER
            return OptionDescriptors.TryGetValue(argId, out optionDescriptor);
#else
            if (OptionDescriptors.Contains(argId))
            {
                optionDescriptor = OptionDescriptors[argId];
                return true;
            }
            else
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                optionDescriptor = default;
                return false;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
#endif
        }
    }
}