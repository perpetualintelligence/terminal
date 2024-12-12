/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Checkers
{
    /// <summary>
    /// An abstraction to check a <see cref="IValue"/>.
    /// </summary>
    public interface IValueChecker<T> where T : IValue
    {
        /// <summary>
        /// Checks the option value.
        /// </summary>
        /// <param name="value">The entity to check.</param>
        /// <returns></returns>
        Task CheckValueAsync(T value);

        /// <summary>
        /// Returns the underlying checker raw type.
        /// </summary>
        /// <returns></returns>
        public Type GetRawType();
    }
}
