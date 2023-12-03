/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Commands.Checkers
{
    /// <summary>
    /// The argument checker result.
    /// </summary>
    /// <seealso cref="IArgumentChecker"/>
    /// <seealso cref="ArgumentCheckerContext"/>
    public class ArgumentCheckerResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="mappedType">The mapped type to validate the option.</param>
        public ArgumentCheckerResult(Type mappedType)
        {
            MappedType = mappedType ?? throw new ArgumentNullException(nameof(mappedType));
        }

        /// <summary>
        /// The mapped system type.
        /// </summary>
        public Type MappedType { get; }
    }
}