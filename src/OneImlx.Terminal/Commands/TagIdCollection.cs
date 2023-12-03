/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// Defines a collection of tags for a command.
    /// </summary>
    public sealed class TagIdCollection : List<string>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="tags">The tags.</param>
        public TagIdCollection(params string[] tags) : base(tags)
        {
        }
    }
}