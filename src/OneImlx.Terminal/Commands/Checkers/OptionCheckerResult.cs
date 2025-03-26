/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Commands.Checkers
{
    /// <summary>
    /// The option checker result.
    /// </summary>
    /// <seealso cref="IOptionChecker"/>
    /// <seealso cref="CommandContext"/>
    public class OptionCheckerResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="mappedType">The mapped type to validate the option.</param>
        public OptionCheckerResult(Type mappedType)
        {
            MappedType = mappedType ?? throw new ArgumentNullException(nameof(mappedType));
        }

        /// <summary>
        /// The mapped system type.
        /// </summary>
        public Type MappedType { get; }
    }
}
