/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// As abstraction of a value.
    /// </summary>
    public interface IValue
    {
        /// <summary>
        /// The identifier.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The data type.
        /// </summary>
        string DataType { get; }

        /// <summary>
        /// The actual value.
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Changes the value's  <see cref="Type"/>.
        /// </summary>
        /// <param name="type"></param>
        void ChangeValueType(Type type);
    }
}