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
            PublishedAssemblies = [];
        }

        /// <summary>
        /// The collection of published assembly with extension and its publish folder.
        /// </summary>
        /// <remarks>
        /// Key is the published assembly file name with extension, and value is its publish folder that contains all the dependencies and the assembly itself.
        /// </remarks>
        public Dictionary<string, string> PublishedAssemblies { get; }
    }
}