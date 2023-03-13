/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// An abstraction to check an argument value.
    /// </summary>
    public interface IArgumentValueChecker
    {
        /// <summary>
        /// Checks the argument value.
        /// </summary>
        /// <param name="argument">The argument to check.</param>
        /// <returns></returns>
        Task CheckAsync(Option argument);

        /// <summary>
        /// Returns the underlying checker raw type.
        /// </summary>
        /// <returns></returns>
        public Type GetRawType();
    }
}