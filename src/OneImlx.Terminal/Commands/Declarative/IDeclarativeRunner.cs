/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Commands.Declarative
{
    /// <summary>
    /// Specifies a runner that provides declarative command and option descriptors.
    /// </summary>
    /// <remarks>
    /// The DI engine uses reflection to identify all the declarative runners and populate the command and
    /// option descriptors.
    /// </remarks>
    public interface IDeclarativeRunner
    {
    }
}