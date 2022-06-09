/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Commands.Declarative
{
    /// <summary>
    /// Specifies a target that provides declarative command and argument descriptors.
    /// </summary>
    /// <remarks>
    /// The <c>pi-cli</c> DI engine uses reflection to identify all the declarative targets and populate the command and
    /// argument descriptors.
    /// </remarks>
    public interface IDeclarativeTarget
    {
    }
}
