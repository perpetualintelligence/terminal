/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// Defines a collection of owners for a command.
    /// </summary>
    public sealed class OwnerCollection : List<string>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="owners">The owner identifiers.</param>
        public OwnerCollection(params string[] owners) : base(owners)
        {
        }
    }
}