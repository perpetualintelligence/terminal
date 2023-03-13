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
    /// An immutable <c>pi-cli</c> command. A command is a specific action or a set of actions that a user or an
    /// application requests the underlying system to perform. It can be a simple action such as invoking a system
    /// method or an OS command or representing a complex operation that calls a set of protected APIs over the internal
    /// or external network. A command can virtually do anything in the context of your application or service.
    /// </summary>
    /// <seealso cref="CommandDescriptor"/>
    /// <seealso cref="Option"/>
    /// <seealso cref="Arguments"/>
    public sealed class Command
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="commandDescriptor"></param>
        /// <param name="arguments"></param>
        public Command(CommandDescriptor commandDescriptor, Options? arguments = null)
        {
            Descriptor = commandDescriptor;
            Arguments = arguments;
        }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public CommandDescriptor Descriptor { get; }

        /// <summary>
        /// The command arguments.
        /// </summary>
        public Options? Arguments { get; }

        /// <summary>
        /// The command custom properties.
        /// </summary>
        public Dictionary<string, object>? CustomProperties => Descriptor.CustomProperties;

        /// <summary>
        /// The command description.
        /// </summary>
        public string? Description => Descriptor.Description;

        /// <summary>
        /// The command id unique.
        /// </summary>
        public string? Id => Descriptor.Id;

        /// <summary>
        /// The command name.
        /// </summary>
        public string Name => Descriptor.Name;

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
                throw new ErrorException(Errors.UnsupportedArgument, "The argument is not supported. argument={0}", id);
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
        public bool TryGetArgument(string id, out Option argument)
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