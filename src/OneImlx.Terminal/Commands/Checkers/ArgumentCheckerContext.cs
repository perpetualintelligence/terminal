/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Commands.Checkers
{
    /// <summary>
    /// The argument checker context.
    /// </summary>
    /// <seealso cref="IOptionChecker"/>
    /// <seealso cref="OptionCheckerResult"/>
    public class ArgumentCheckerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArgumentCheckerContext(Argument argument)
        {
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
        }

        /// <summary>
        /// The argument to check.
        /// </summary>
        public Argument Argument { get; set; }
    }
}