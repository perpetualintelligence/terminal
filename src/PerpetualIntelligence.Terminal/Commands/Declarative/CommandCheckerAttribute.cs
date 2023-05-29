/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Commands.Declarative
{
    /// <summary>
    /// Declares a command checker.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CommandCheckerAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="checker">The command checker.</param>
        public CommandCheckerAttribute(Type checker)
        {
            Checker = checker ?? throw new ArgumentNullException(nameof(checker));
        }

        /// <summary>
        /// The command checker type.
        /// </summary>
        public Type Checker { get; }
    }
}
