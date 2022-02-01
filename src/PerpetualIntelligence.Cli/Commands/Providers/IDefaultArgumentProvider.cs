﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    /// <summary>
    /// An abstraction to provide an argument's default value.
    /// </summary>
    public interface IDefaultArgumentProvider : IProvider<DefaultArgumentProviderContext, DefaultArgumentProviderResult>
    {
    }
}
