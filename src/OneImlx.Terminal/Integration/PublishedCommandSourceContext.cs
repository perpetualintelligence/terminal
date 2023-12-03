/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;

namespace OneImlx.Terminal.Integration
{
    /// <summary>
    /// The context for <see cref="PublishedCommandSource"/>
    /// </summary>
    public sealed class PublishedCommandSourceContext
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public PublishedCommandSourceContext()
        {
            PublishedAssemblies = new Dictionary<string, string>();
        }

        /// <summary>
        /// The collection of published assembly with extension and its publish folder.
        /// </summary>
        public Dictionary<string, string> PublishedAssemblies { get; }
    }
}