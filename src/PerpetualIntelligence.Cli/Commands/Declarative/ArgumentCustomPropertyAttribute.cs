/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Declarative
{
    /// <summary>
    /// Declares an <see cref="ArgumentDescriptor"/> custom property.
    /// </summary>
    /// <remarks>Each custom property must have a unique key within an argument.</remarks>
    /// <seealso cref="ArgumentDescriptor.Properties"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ArgumentCustomPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="argId">The argument identifier.</param>
        /// <param name="key">The custom property key.</param>
        /// <param name="value">The custom property value.</param>
        public ArgumentCustomPropertyAttribute(string argId, string key, object value)
        {
            ArgId = argId;
            Key = key;
            Value = value;
        }

        /// <summary>
        /// The argument identifier to identify <see cref="ArgumentDescriptor"/>.
        /// </summary>
        public string ArgId { get; }

        /// <summary>
        /// The custom property key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The custom property value.
        /// </summary>
        public object Value { get; set; }
    }
}
