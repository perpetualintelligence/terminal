/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Attributes;
using PerpetualIntelligence.Shared.Exceptions;
using System.Collections.Generic;

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// An immutable <c>pi-cli</c> command. A command is a specific action or a set of actions that a user or an
    /// application requests the underlying system to perform. It can be a simple action such as invoking a system
    /// method or an OS command or representing a complex operation that calls a set of protected APIs over the internal
    /// or external network. A command can virtually do anything in the context of your application or service.
    /// </summary>
    /// <seealso cref="CommandDescriptor"/>
    /// <seealso cref="Option"/>
    /// <seealso cref="Options"/>
    public sealed class Command
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="commandRoute">The command route.</param>
        /// <param name="commandDescriptor">The command descriptor.</param>
        /// <param name="options">The command options.</param>
        public Command(CommandRoute commandRoute, CommandDescriptor commandDescriptor, Options? options = null)
        {
            Route = commandRoute ?? throw new System.ArgumentNullException(nameof(commandRoute));
            Descriptor = commandDescriptor ?? throw new System.ArgumentNullException(nameof(commandDescriptor));
            Options = options;
        }

        /// <summary>
        /// The command route.
        /// </summary>
        public CommandRoute Route { get; }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public CommandDescriptor Descriptor { get; }

        /// <summary>
        /// The command options.
        /// </summary>
        public Options? Options { get; }

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
        /// Gets the optional option value for the specified identifier.
        /// </summary>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <returns>The optional option value.</returns>
        public TValue? GetOptionalOptionValue<TValue>(string id)
        {
            if (Options == null)
            {
                return default;
            }

            if (Options.Contains(id))
            {
                return Options.GetValue<TValue>(id);
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Gets the required option value for the specified identifier.
        /// </summary>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <returns>The option value.</returns>
        /// <exception cref="ErrorException">If the option is not supported.</exception>
        public TValue GetRequiredOptionValue<TValue>(string id)
        {
            if (Options == null)
            {
                throw new ErrorException(Errors.UnsupportedOption, "The option is not supported. option={0}", id);
            }

            return Options.GetValue<TValue>(id);
        }

        /// <summary>
        /// Attempts to find an option.
        /// </summary>
        /// <param name="id">The option identifier.</param>
        /// <param name="option">The option if found in the collection.</param>
        /// <returns><c>true</c> if an option exist in the collection, otherwise <c>false</c>.</returns>
        [WriteUnitTest]
        public bool TryGetOption(string id, out Option option)
        {
            if (Options == null)
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                option = default;
                return false;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }

#if NETSTANDARD2_1_OR_GREATER
            return Options.TryGetValue(id, out option);
#else
            if (Options.Contains(id))
            {
                option = Options[id];
                return true;
            }
            else
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                option = default;
                return false;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
#endif
        }
    }
}