/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// Defines a collection of owners for a command.
    /// </summary>
    /// <remarks>
    /// The <see cref="OwnerIdCollection"/> is used to identify an injected collection of owners in the service
    /// collection by various builders.
    /// </remarks>
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
