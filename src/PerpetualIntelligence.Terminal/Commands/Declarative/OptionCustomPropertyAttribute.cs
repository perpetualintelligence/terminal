/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Commands.Declarative
{
    /// <summary>
    /// Declares an <see cref="OptionDescriptor"/> custom property.
    /// </summary>
    /// <remarks>Each custom property must have a unique key within an option.</remarks>
    /// <seealso cref="OptionDescriptor.CustomProperties"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class OptionCustomPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="argId">The option identifier.</param>
        /// <param name="key">The custom property key.</param>
        /// <param name="value">The custom property value.</param>
        public OptionCustomPropertyAttribute(string argId, string key, object value)
        {
            if (string.IsNullOrWhiteSpace(argId))
            {
                throw new ArgumentException($"'{nameof(argId)}' cannot be null or whitespace.", nameof(argId));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
            }

            ArgId = argId;
            Key = key;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The option identifier to identify <see cref="OptionDescriptor"/>.
        /// </summary>
        public string ArgId { get; }

        /// <summary>
        /// The custom property key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The custom property value.
        /// </summary>
        public object Value { get; }
    }
}
