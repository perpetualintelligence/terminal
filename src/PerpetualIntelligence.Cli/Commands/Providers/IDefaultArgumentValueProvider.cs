/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    /// <summary>
    /// An abstraction to provide an option's default value.
    /// </summary>
    public interface IDefaultArgumentValueProvider : IProvider<DefaultArgumentValueProviderContext, DefaultArgumentValueProviderResult>
    {
    }
}
