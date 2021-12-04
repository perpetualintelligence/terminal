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
        /// <param name="argumentIdentities">The command argument identities.</param>
        /// <param name="description">The command description.</param>
        /// <param name="checker">The command checker.</param>
        /// <param name="runner">The command runner.</param>
        public CommandIdentity(string id, string name, string prefix, ArgumentIdentities? argumentIdentities = null, string? description = null, Type? checker = null, Type? runner = null)
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
            ArgumentIdentities = argumentIdentities;
            Checker = checker;
            Runner = runner;
        }

        /// <summary>
        /// The command arguments identity.
        /// </summary>
        public ArgumentIdentities? ArgumentIdentities { get; set; }

        /// <summary>
        /// The command checker.
        /// </summary>
        /// <remarks>This is set during startup configuration via <see cref="ICliBuilderExtensions.AddCommandIdentity{TRunner, TChecker}(OneImlx.Configuration.ICliBuilder, CommandIdentity)"/>.</remarks>
        public Type? Checker { get; set; }

        // <summary>
        /// The command description. </summary>
        public string? Description { get; set; }

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
