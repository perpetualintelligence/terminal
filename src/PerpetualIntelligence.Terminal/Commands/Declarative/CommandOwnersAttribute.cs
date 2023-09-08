/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Commands.Declarative
{
    /// <summary>
    /// Declares a command owners. A command owner is a group or a root.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CommandOwnersAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="owners">The command owner identifiers.</param>
        public CommandOwnersAttribute(params string[] owners)
        {
            Owners = new OwnerCollection(owners);
        }

        /// <summary>
        /// The command owner identifiers.
        /// </summary>
        /// <seealso cref="CommandDescriptor.Id"/>
        public OwnerCollection Owners { get; }
    }
}