/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// Defines a collection of owners for a command.
    /// </summary>
    public sealed class OwnerIdCollection : List<string>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="ownerIds">The owner identifiers.</param>
        public OwnerIdCollection(params string[] ownerIds) : base(ownerIds)
        {
        }
    }
}