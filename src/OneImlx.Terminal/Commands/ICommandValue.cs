/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// As abstraction of a command value.
    /// </summary>
    /// <remarks>A command value is a value that is passed to a command. It can be an argument, option or a flag.</remarks>
    public interface ICommandValue
    {
        /// <summary>
        /// The data type.
        /// </summary>
        string DataType { get; }

        /// <summary>
        /// The identifier.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The actual value.
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Changes the value's <see cref="Type"/>.
        /// </summary>
        /// <param name="type"></param>
        void ChangeValueType(Type type);
    }
}
