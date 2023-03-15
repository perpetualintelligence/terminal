/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// An abstraction to check an option value.
    /// </summary>
    public interface IOptionValueChecker
    {
        /// <summary>
        /// Checks the option value.
        /// </summary>
        /// <param name="option">The option to check.</param>
        /// <returns></returns>
        Task CheckAsync(Option option);

        /// <summary>
        /// Returns the underlying checker raw type.
        /// </summary>
        /// <returns></returns>
        public Type GetRawType();
    }
}