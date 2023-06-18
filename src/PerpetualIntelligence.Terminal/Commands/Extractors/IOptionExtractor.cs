/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    /// <summary>
    /// An abstraction to extract an <see cref="Option"/>.
    /// </summary>
    public interface IOptionExtractor
    {
        /// <summary>
        /// Extracts <see cref="Option"/> asynchronously.
        /// </summary>
        /// <param name="context">The option extraction context.</param>
        /// <returns>The <see cref="OptionExtractorResult"/> instance.</returns>
        public Task<OptionExtractorResult> ExtractAsync(OptionExtractorContext context);
    }
}