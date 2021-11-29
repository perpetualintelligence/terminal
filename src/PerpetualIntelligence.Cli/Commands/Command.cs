/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Attributes;
using System.Text.Json.Serialization;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// A context specific command.
    /// </summary>
    /// <remarks>
    /// The command name can be same within multiple command groups. The command id is unique across all command groups.
    /// </remarks>
    /// <seealso cref="CommandGroup"/>
    /// <seealso cref="Commands.Arguments"/>
    public sealed class Command
    {
        /// <summary>
        /// The command arguments.
        /// </summary>
        [JsonPropertyName("arguments")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Arguments? Arguments { get; set; }

        /// <summary>
        /// Determines if the command is checked.
        /// </summary>
        /// <seealso cref="ICommandChecker"/>
        [JsonIgnore]
        [ToUnitTest("JsonIgnore and Internal and design.")]
        public bool Checked { get; internal set; }

        /// <summary>
        /// The command description.
        /// </summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>
        /// The command group id.
        /// </summary>
        [JsonPropertyName("group_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? GroupId { get; set; }

        /// <summary>
        /// The command id unique.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id { get; set; }

        /// <summary>
        /// The command name.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name;
    }
}
