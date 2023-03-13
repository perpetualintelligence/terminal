/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// The argument checker context.
    /// </summary>
    /// <seealso cref="IArgumentChecker"/>
    /// <seealso cref="ArgumentCheckerResult"/>
    public class ArgumentCheckerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="argumentDescriptor">The argument descriptor.</param>
        /// <param name="argument">The argument.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArgumentCheckerContext(OptionDescriptor argumentDescriptor, Option argument)
        {
            ArgumentDescriptor = argumentDescriptor ?? throw new ArgumentNullException(nameof(argumentDescriptor));
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
        }

        /// <summary>
        /// The argument to check.
        /// </summary>
        public Option Argument { get; set; }

        /// <summary>
        /// The argument descriptor.
        /// </summary>
        public OptionDescriptor ArgumentDescriptor { get; }
    }
}
