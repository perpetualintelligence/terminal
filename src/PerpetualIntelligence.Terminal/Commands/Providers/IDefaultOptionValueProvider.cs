/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Providers
{
    /// <summary>
    /// An abstraction to provide an option's default value.
    /// </summary>
    public interface IDefaultOptionValueProvider
    {
        /// <summary>
        /// Provides an option's default value asynchronously.
        /// </summary>
        /// <param name="defaultOptionValueProviderContext">The option's default value context.</param>
        /// <returns>The <see cref="DefaultOptionValueProviderResult"/> instance.</returns>
        Task<DefaultOptionValueProviderResult> ProvideAsync(DefaultOptionValueProviderContext defaultOptionValueProviderContext);
    }
}