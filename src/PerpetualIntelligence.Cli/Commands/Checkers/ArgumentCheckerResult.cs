/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;
using System;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// The argument checker result.
    /// </summary>
    /// <seealso cref="IArgumentChecker"/>
    /// <seealso cref="ArgumentCheckerContext"/>
    public class ArgumentCheckerResult : OneImlxResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public ArgumentCheckerResult()
        {
        }

        /// <summary>
        /// The mapped system type.
        /// </summary>
        public Type? MappedType { get; set; }
    }
}
