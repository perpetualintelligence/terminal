/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands.Declarative
{
    /// <summary>
    /// Specifies a target that provides declarative command and option descriptors.
    /// </summary>
    /// <remarks>
    /// The <c>pi-cli</c> DI engine uses reflection to identify all the declarative targets and populate the command and
    /// option descriptors.
    /// </remarks>
    public interface IDeclarativeTarget
    {
    }
}
