/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/


using PerpetualIntelligence.Shared.Attributes;
using PerpetualIntelligence.Shared.Exceptions;
using System.Collections.Generic;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// An immutable <c>cli</c> command.
    /// </summary>
    /// <seealso cref="Argument"/>
    public sealed class Command
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="id">The command id.</param>
        /// <param name="name">The command name.</param>
        /// <param name="description">The command description.</param>
        /// <param name="arguments">The command arguments.</param>
        /// <param name="properties">The command properties.</param>
        public Command(string id, string name, string description, Arguments? arguments = null, Dictionary<string, object>? properties = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new System.ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new System.ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
            }

            Id = id;
            Name = name;
            Description = description ?? throw new System.ArgumentNullException(nameof(description));
            Arguments = arguments;
            Properties = properties;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="commandDescriptor"></param>
        /// <param name="arguments"></param>
        /// <param name="properties"></param>
        public Command(CommandDescriptor commandDescriptor, Arguments? arguments = null, Dictionary<string, object>? properties = null)
        {
            Id = commandDescriptor.Id;
            Name = commandDescriptor.Name;
            Description = commandDescriptor.Description;
            Arguments = arguments;
            Properties = properties;
        }

        /// <summary>
        /// The command arguments.
        /// </summary>
        public Arguments? Arguments { get; }

        /// <summary>
        /// The command description.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// The command id unique.
        /// </summary>
        public string? Id { get; }

        /// <summary>
        /// The command name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The command custom properties.
        /// </summary>
        public Dictionary<string, object>? Properties { get; }

        /// <summary>
        /// Gets the optional argument value for the specified identifier.
        /// </summary>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <returns>The optional argument value.</returns>
        public TValue? GetOptionalArgumentValue<TValue>(string id)
        {
            if (Arguments == null)
            {
                return default;
            }

            if (Arguments.Contains(id))
            {
                return Arguments.GetValue<TValue>(id);
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Gets the required argument value for the specified identifier.
        /// </summary>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <returns>The argument value.</returns>
        /// <exception cref="ErrorException">If the argument is not supported.</exception>
        public TValue GetRequiredArgumentValue<TValue>(string id)
        {
            if (Arguments == null)
            {
                throw new ErrorException(Errors.UnsupportedArgument, "The argument is not supported. argument={1}", id);
            }

            return Arguments.GetValue<TValue>(id);
        }

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
    }
}
