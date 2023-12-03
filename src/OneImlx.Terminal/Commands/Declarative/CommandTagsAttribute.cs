/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Commands.Declarative
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
            Tags = new TagIdCollection(tags);
        }

        /// <summary>
        /// The command tags.
        /// </summary>
        public TagIdCollection Tags { get; }
    }
}