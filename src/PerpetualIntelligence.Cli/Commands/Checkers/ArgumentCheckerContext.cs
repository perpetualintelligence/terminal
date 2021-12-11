/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
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
        /// <param name="argumentIdentity">The argument identity.</param>
        /// <param name="argument">The argument.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArgumentCheckerContext(ArgumentIdentity argumentIdentity, Argument argument)
        {
            ArgumentIdentity = argumentIdentity ?? throw new ArgumentNullException(nameof(argumentIdentity));
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
        }

        /// <summary>
        /// The argument to check.
        /// </summary>
        public Argument Argument { get; set; }

        /// <summary>
        /// The argument identity.
        /// </summary>
        public ArgumentIdentity ArgumentIdentity { get; }
    }
}
