/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Declarative
{
    /// <summary>
    /// Declares a <see cref="CommandDescriptor"/> custom property.
    /// </summary>
    /// <remarks>Each custom property must have a unique key within a command.</remarks>
    /// <seealso cref="CommandDescriptor.Properties"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class CommandCustomPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        public CommandCustomPropertyAttribute(string key, object value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// The property key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The property value.
        /// </summary>
        public object Value { get; }
    }
}
