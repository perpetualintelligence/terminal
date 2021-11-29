/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Attributes;
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
        /// <param name="requestHandler">The command request handler.</param>
        [ToUnitTest("null checks")]
        public CommandIdentity(string id, string name, Type requestHandler)
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
            RequestHandler = requestHandler ?? throw new ArgumentNullException(nameof(requestHandler));
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="groupId">The command group id.</param>
        /// <param name="id">The command id.</param>
        /// <param name="name">The command named.</param>
        /// <param name="requestHandler">The command request handler.</param>
        public CommandIdentity(string groupId, string id, string name, Type requestHandler) : this(id, name, requestHandler)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                throw new ArgumentException($"'{nameof(groupId)}' cannot be null or whitespace.", nameof(groupId));
            }

            GroupId = groupId;
            RequestHandler = requestHandler;
        }

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
        /// The command request handler.
        /// </summary>
        public Type RequestHandler { get; set; }
    }
}
