﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Abstractions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Licensing
{
    /// <summary>
    /// An abstraction to extract the software <see cref="License"/>.
    /// </summary>
    public interface ILicenseExtractor : IExtractor<LicenseExtractorContext, LicenseExtractorResult>
    {
        /// <summary>
        /// Gets the extracted license asynchronously.
        /// </summary>
        public Task<License?> GetLicenseAsync();
    }
}