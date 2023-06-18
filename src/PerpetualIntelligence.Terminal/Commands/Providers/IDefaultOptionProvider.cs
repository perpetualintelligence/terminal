/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Providers
{
    /// <summary>
    /// An abstraction to provide a default option.
    /// </summary>
    public interface IDefaultOptionProvider
    {
        /// <summary>
        /// Provides a default option asynchronously.
        /// </summary>
        /// <param name="defaultOptionValueProviderContext">The default option context.</param>
        /// <returns>The <see cref="DefaultOptionProviderResult"/> instance.</returns>
        Task<DefaultOptionProviderResult> ProvideAsync(DefaultOptionProviderContext defaultOptionValueProviderContext);
    }
}