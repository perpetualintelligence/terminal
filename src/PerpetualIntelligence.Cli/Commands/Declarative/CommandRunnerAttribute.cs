/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Declarative
{
    /// <summary>
    /// Declares a command checker.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CommandRunnerAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="runner">The command runner.</param>
        public CommandRunnerAttribute(Type runner)
        {
            Runner = runner ?? throw new ArgumentNullException(nameof(runner));
        }

        /// <summary>
        /// The command runner type.
        /// </summary>
        public Type Runner { get; set; }
    }
}
