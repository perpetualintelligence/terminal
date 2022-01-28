/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
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
        public ArgumentCheckerContext(ArgumentDescriptor argumentDescriptor, Argument argument)
        {
            ArgumentDescriptor = argumentDescriptor ?? throw new ArgumentNullException(nameof(argumentDescriptor));
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
        }

        /// <summary>
        /// The argument to check.
        /// </summary>
        public Argument Argument { get; set; }

        /// <summary>
        /// The argument descriptor.
        /// </summary>
        public ArgumentDescriptor ArgumentDescriptor { get; }
    }
}
