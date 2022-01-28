/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Attributes;
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
        /// Initialize a new instance from the specified command descriptor.
        /// </summary>
        /// <param name="commandDescriptor">The command descriptor.</param>
        public Command(CommandDescriptor commandDescriptor)
        {
            Id = commandDescriptor.Id;
            Name = commandDescriptor.Name;
            Description = commandDescriptor.Description;

            if (commandDescriptor.ArgumentDescriptors != null)
            {
                Arguments = new Arguments();
                foreach (ArgumentDescriptor argument in commandDescriptor.ArgumentDescriptors)
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
        /// Attempts to find an argument.
        /// </summary>
        /// <param name="id">The argument identifier.</param>
        /// <param name="argument">The argument if found in the collection.</param>
        /// <returns><c>true</c> if an argument exist in the collection, otherwise <c>false</c>.</returns>
        [WriteUnitTest]
        public bool TryGetArgument(string id, out Argument argument)
        {
            if (Arguments == null)
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                argument = default;
                return false;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }

#if NETSTANDARD2_1_OR_GREATER
            return Arguments.TryGetValue(id, out argument);
#else
            if (Arguments.Contains(id))
            {
                argument = Arguments[id];
                return true;
            }
            else
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                argument = default;
                return false;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
#endif
        }

        /// <summary>
        /// The command name.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name;
    }
}
