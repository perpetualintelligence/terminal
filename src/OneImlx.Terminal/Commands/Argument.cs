/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;

namespace OneImlx.Terminal.Commands
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
    /// <see cref="ArgumentDescriptors"/> collection that contains attributes with unique ids.
    /// </para>
    /// </remarks>
    /// <seealso cref="Command"/>
    public sealed class Argument : IEquatable<Argument?>, IKeyAsId, ICommandValue
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
        /// The option data type.
        /// </summary>
        public string DataType => Descriptor.DataType;

        /// <summary>
        /// The argument descriptor.
        /// </summary>
        public ArgumentDescriptor Descriptor { get; }

        /// <summary>
        /// The argument id.
        /// </summary>
        /// <remarks>The argument id is unique with in a command.</remarks>
        public string Id => Descriptor.Id;

        /// <summary>
        /// The argument value.
        /// </summary>
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
        /// Changes the argument value to the specified type.
        /// </summary>
        /// <param name="type">The new type to use.</param>
        public void ChangeValueType(Type type)
        {
            Value = Convert.ChangeType(Value, type);
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
    }
}
