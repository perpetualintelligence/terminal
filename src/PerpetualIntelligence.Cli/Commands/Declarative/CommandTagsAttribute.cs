/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Declarative
{
    /// <summary>
    /// Declares the command tags.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CommandTagsAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="tags">The tags.</param>
        public CommandTagsAttribute(params string[] tags)
        {
            Tags = tags;
        }

        /// <summary>
        /// The command tags.
        /// </summary>
        public string[] Tags { get; }
    }
}
