/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
{
    /// <summary>
    /// The option checker context.
    /// </summary>
    /// <seealso cref="IOptionChecker"/>
    /// <seealso cref="OptionCheckerResult"/>
    public class OptionCheckerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public OptionCheckerContext(Option option)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));
        }

        /// <summary>
        /// The option to check.
        /// </summary>
        public Option Option { get; set; }
    }
}