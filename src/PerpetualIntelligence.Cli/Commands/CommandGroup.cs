/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System.Text.Json.Serialization;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// Represents a command group.
    /// </summary>
    /// <remarks>The command group has unique id and name.</remarks>
    public class CommandGroup
    {
        /// <summary>
        /// The commands supported by this command group.
        /// </summary>
        [JsonPropertyName("command_ids")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[]? CommandIds { get; set; }

        /// <summary>
        /// The command group id.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id;
    }
}
