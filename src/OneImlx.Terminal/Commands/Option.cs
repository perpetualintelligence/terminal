/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// The <see cref="Option"/> class is a runtime validated representation of an actual command option and its value
    /// passed by a user or an application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An option id is always unique within a command. By design <see cref="Option"/> implements the default equality
    /// <see cref="IEquatable{T}"/> and <see cref="GetHashCode()"/> using <see cref="Id"/> property. Thus, two options
    /// with the same id are equal irrespective of other property values. This is done to improve performance during
    /// lookup and avoid multiple options with same identifiers.
    /// </para>
    /// <para>
    /// The options can have same ids across multiple commands. Each <see cref="Command"/> has
    /// <see cref="OptionDescriptors"/> collection that contains options with unique ids.
    /// </para>
    /// </remarks>
    /// <seealso cref="Command"/>
    public sealed class Option : IEquatable<Option?>, IValue
    {
        /// <summary>
        /// Initialize a new instance..
        /// </summary>
        /// <param name="optionDescriptor">The option descriptor.</param>
        /// <param name="value">The option value.</param>
        /// <param name="byAlias">Determines whether the option is identifier by its alias.</param>
        public Option(OptionDescriptor optionDescriptor, object value, bool byAlias = false)
        {
            Value = value;
            ByAlias = byAlias;
            Descriptor = optionDescriptor;
        }

        /// <summary>
        /// The option alias.
        /// </summary>
        [JsonIgnore]
        public string? Alias => Descriptor.Alias;

        /// <summary>
        /// The option data type.
        /// </summary>
        [JsonIgnore]
        public string DataType => Descriptor.DataType;

        /// <summary>
        /// The option description.
        /// </summary>
        public string? Description => Descriptor.Description;

        /// <summary>
        /// The option descriptor.
        /// </summary>
        public OptionDescriptor Descriptor { get; }

        /// <summary>
        /// The option id.
        /// </summary>
        /// <remarks>The option id is unique with in a command.</remarks>
        public string Id => Descriptor.Id;

        /// <summary>
        /// The option value.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Determines whether the option is identifier by its alias.
        /// </summary>
        public bool ByAlias { get; }

        /// <summary>
        /// Indicates whether the current option is not equal to another option.
        /// </summary>
        /// <param name="left">The left option.</param>
        /// <param name="right">The right option.</param>
        /// <returns><c>true</c> if the current option is not equal to the other option; otherwise, false.</returns>
        public static bool operator !=(Option? left, Option? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Indicates whether the current option is equal to another option.
        /// </summary>
        /// <param name="left">The left option.</param>
        /// <param name="right">The right option.</param>
        /// <returns><c>true</c> if the current option is equal to the other option; otherwise, false.</returns>
        public static bool operator ==(Option? left, Option? right)
        {
            return EqualityComparer<Option?>.Default.Equals(left, right);
        }

        /// <summary>
        /// Changes the option value to the specified type.
        /// </summary>
        /// <param name="type">The new type to use.</param>
        public void ChangeValueType(Type type)
        {
            Value = Convert.ChangeType(Value, type);
        }

        /// <summary>
        /// Indicates whether the current option is equal to another option.
        /// </summary>
        /// <param name="obj">The other option.</param>
        /// <returns><c>true</c> if the current option is equal to the other option; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as Option);
        }

        /// <summary>
        /// Indicates whether the current option is equal to another option.
        /// </summary>
        /// <param name="other">The other option.</param>
        /// <returns><c>true</c> if the current option is equal to the other option; otherwise, false.</returns>
        public bool Equals(Option? other)
        {
            return other != null &&
                   Id == other.Id;
        }

        /// <summary>
        /// Returns the hash code for this option.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
