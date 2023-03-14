/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Checkers
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
        /// <param name="argumentDescriptor">The option descriptor.</param>
        /// <param name="option">The option.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public OptionCheckerContext(OptionDescriptor argumentDescriptor, Option option)
        {
            ArgumentDescriptor = argumentDescriptor ?? throw new ArgumentNullException(nameof(argumentDescriptor));
            Argument = option ?? throw new ArgumentNullException(nameof(option));
        }

        /// <summary>
        /// The option to check.
        /// </summary>
        public Option Argument { get; set; }

        /// <summary>
        /// The option descriptor.
        /// </summary>
        public OptionDescriptor ArgumentDescriptor { get; }
    }
}
