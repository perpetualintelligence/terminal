/*
    Copyright 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The options based command extractor.
    /// </summary>
    public class OptionsCommandExtractor : ICommandExtractor
    {
        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task<CommandExtractorResult> ExtractAsync(CommandExtractorContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
