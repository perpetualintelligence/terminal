/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The <see cref="Argument"/> class is a runtime validated representation of an actual command argument and its
    /// value passed by a user or an application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An argument id is always unique within a command. By design <see cref="Argument"/> implements the default
    /// equality <see cref="IEquatable{T}"/> and <see cref="GetHashCode()"/> using <see cref="Id"/> property. Thus, two
    /// arguments with the same id are equal irrespective of other property values. This is done to improve performance
    /// during lookup and avoid multiple arguments with same identifiers.
    /// </para>
    /// <para>
    /// The arguments can have same ids across multiple commands. Each <see cref="Command"/> has
    /// <see cref="ArgumentDescriptors"/> collection that contains arguments with unique ids.
    /// </para>
    /// </remarks>
    /// <seealso cref="Command"/>
    public sealed class Argument : IEquatable<Argument?>
    {
        /// <summary>
        /// Initialize a new instance..
        /// </summary>
        /// <param name="argumentDescriptor">The argument descriptor.</param>
        /// <param name="value">The argument value.</param>
        public Argument(ArgumentDescriptor argumentDescriptor, object value)
        {
            Value = value;
            Descriptor = argumentDescriptor;
        }

        /// <summary>
        /// The argument descriptor.
        /// </summary>
        [JsonPropertyName("descriptor")]
        public ArgumentDescriptor Descriptor { get; }

        /// <summary>
        /// The argument alias.
        /// </summary>
        [JsonIgnore]
        public string? Alias => Descriptor.Alias;

        /// <summary>
        /// The argument custom data type.
        /// </summary>
        /// <remarks>This custom data type is used only if the <see cref="DataType"/> property is set to <see cref="DataType.Custom"/>.</remarks>
        [JsonIgnore]
        public string? CustomDataType => Descriptor.CustomDataType;

        /// <summary>
        /// The argument data type. Defaults to <see cref="DataType.Text"/>.
        /// </summary>
        [JsonIgnore]
        public DataType DataType => Descriptor.DataType;

        /// <summary>
        /// The argument description.
        /// </summary>
        [JsonIgnore]
        public string? Description => Descriptor.Description;

        /// <summary>
        /// The argument id.
        /// </summary>
        /// <remarks>The argument id is unique with in a command.</remarks>
        [JsonIgnore]
        public string Id => Descriptor.Id;

        /// <summary>
        /// The argument custom properties.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, object>? CustomProperties => Descriptor.CustomProperties;

        /// <summary>
        /// The argument value.
        /// </summary>
        [JsonPropertyName("value")]
        public object Value { get; set; }

        /// <summary>
        /// Indicates whether the current argument is not equal to another argument.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns><c>true</c> if the current argument is not equal to the other argument; otherwise, false.</returns>
        public static bool operator !=(Argument? left, Argument? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Indicates whether the current argument is equal to another argument.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns><c>true</c> if the current argument is equal to the other argument; otherwise, false.</returns>
        public static bool operator ==(Argument? left, Argument? right)
        {
            return EqualityComparer<Argument?>.Default.Equals(left, right);
        }

        /// <summary>
        /// Indicates whether the current argument is equal to another argument.
        /// </summary>
        /// <param name="obj">The other argument.</param>
        /// <returns><c>true</c> if the current argument is equal to the other argument; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as Argument);
        }

        /// <summary>
        /// Indicates whether the current argument is equal to another argument.
        /// </summary>
        /// <param name="other">The other argument.</param>
        /// <returns><c>true</c> if the current argument is equal to the other argument; otherwise, false.</returns>
        public bool Equals(Argument? other)
        {
            return other != null &&
                   Id == other.Id;
        }

        /// <summary>
        /// Returns the hash code for this argument.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Changes the argument value to the specified type.
        /// </summary>
        /// <param name="type">The new type to use.</param>
        internal void ChangeValueType(Type type)
        {
            Value = Convert.ChangeType(Value, type);
        }
    }
}