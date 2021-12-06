/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

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
        /// Initialize a new instance.
        /// </summary>
        public Command()
        {
        }

        /// <summary>
        /// Initialize a new instance from the specified command identity.
        /// </summary>
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
                    // FOMAC: We dont have access to argument values here !
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    Argument arg = new Argument(argument, null);
                    Arguments.Add(arg);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
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
        /// The command name.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name;
    }
}
