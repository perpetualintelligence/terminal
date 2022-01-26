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
    /// A <c>cli</c> command.
    /// </summary>
    /// <seealso cref="Argument"/>
    public sealed class Command
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public Command()
        {
        }

        /// <summary>
        /// Initialize a new instance from the specified command identity.
        /// </summary>
        /// <param name="commandIdentity">The command identity.</param>
        public Command(CommandIdentity commandIdentity)
        {
            Id = commandIdentity.Id;
            Name = commandIdentity.Name;
            Description = commandIdentity.Description;

            if (commandIdentity.ArgumentIdentities != null)
            {
                Arguments = new Arguments();
                foreach (var argument in commandIdentity.ArgumentIdentities)
                {
                    Argument arg = new(argument, argument.DefaultValue ?? new object());
                    Arguments.Add(arg);
                }
            }
        }

        /// <summary>
        /// The command arguments.
        /// </summary>
        [JsonPropertyName("arguments")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Arguments? Arguments { get; set; }

        /// <summary>
        /// The command description.
        /// </summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>
        /// The command id unique.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id { get; set; }

        /// <summary>
        /// The command custom properties.
        /// </summary>
        [JsonPropertyName("properties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object>? Properties { get; set; }

        /// <summary>
        /// The command name.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name;
    }
}
