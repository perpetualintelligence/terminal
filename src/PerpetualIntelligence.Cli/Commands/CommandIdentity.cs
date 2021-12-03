/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Extensions;
using System;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// Identifies a command uniquely.
    /// </summary>
    public class CommandIdentity
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The command id.</param>
        /// <param name="name">The command name.</param>
        /// <param name="prefix">The command prefix to map the command string.</param>
        /// <param name="arguments">The command arguments.</param>
        public CommandIdentity(string id, string name, string prefix, ArgumentIdentity[]? arguments)
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
            Arguments = arguments;
            Prefix = prefix;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="groupId">The command group id.</param>
        /// <param name="id">The command id.</param>
        /// <param name="name">The command named.</param>
        /// <param name="prefix"></param>
        /// <param name="arguments">The command arguments.</param>
        public CommandIdentity(string groupId, string id, string name, string prefix, ArgumentIdentity[]? arguments) : this(id, name, prefix, arguments)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                throw new ArgumentException($"'{nameof(groupId)}' cannot be null or whitespace.", nameof(groupId));
            }

            GroupId = groupId;
        }

        /// <summary>
        /// The command arguments identity.
        /// </summary>
        public ArgumentIdentity[]? Arguments { get; set; }

        /// <summary>
        /// The command checker.
        /// </summary>
        /// <remarks>This is set during startup configuration via <see cref="ICliBuilderExtensions.AddCommandIdentity{TRunner, TChecker}(OneImlx.Configuration.ICliBuilder, CommandIdentity)"/>.</remarks>
        public Type? Checker { get; set; }

        // <summary>
        /// The command description. </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The command group id.
        /// </summary>
        public string? GroupId { get; set; }

        /// <summary>
        /// The command id.
        /// </summary>
        /// <remarks>The command id is unique across all command group.</remarks>
        public string Id { get; set; }

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
        /// The command runner.
        /// </summary>
        /// <remarks>This is set during startup configuration via <see cref="ICliBuilderExtensions.AddCommandIdentity{TRunner, TChecker}(OneImlx.Configuration.ICliBuilder, CommandIdentity)"/>.</remarks>
        public Type? Runner { get; set; }
    }
}
