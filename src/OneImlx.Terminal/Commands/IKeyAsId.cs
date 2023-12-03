/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// An abstraction of an entity with a key as an identifier.
    /// </summary>
    public interface IKeyAsId
    {
        /// <summary>
        /// The identifier.
        /// </summary>
        public string Id { get; }
    }
}