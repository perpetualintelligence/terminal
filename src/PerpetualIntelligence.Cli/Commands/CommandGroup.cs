/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// A <c>cli</c> command group.
    /// </summary>
    /// <seealso cref="Argument"/>
    public sealed class CommandGroup
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commands">The commands that are part of this group.</param>
        [JsonConstructor]
        public CommandGroup(string[] commands)
        {
            Commands = commands;
        }

        /// <summary>
        /// Initialize a new instance from the specified command identity.
        /// </summary>
        public CommandGroup(CommandIdentity commandIdentity, string[] commands)
        {
            Id = commandIdentity.Id;
            Name = commandIdentity.Name;
            Description = commandIdentity.Description;

            if (commandIdentity.ArgumentIdentities != null)
            {
                Arguments = new Arguments();
                foreach (var argument in commandIdentity.ArgumentIdentities)
                {
                    // FOMAC: We dont have access to argument values here !
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    Argument arg = new Argument(argument, null);
                    Arguments.Add(arg);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
            }
            Commands = commands;
        }

        /// <summary>
        /// The command group arguments.
        /// </summary>
        [JsonPropertyName("arguments")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Arguments? Arguments { get; set; }

        /// <summary>
        /// The commands within this group.
        /// </summary>
        [JsonPropertyName("commands")]
        public string[] Commands { get; set; }

        /// <summary>
        /// The command group description.
        /// </summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>
        /// The command group id unique.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id { get; set; }

        /// <summary>
        /// The command group custom properties.
        /// </summary>
        [JsonPropertyName("properties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object>? Properties { get; set; }

        /// <summary>
        /// The command group name.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name;
    }
}
