/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Shared.Exceptions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Routers.Mocks
{
    internal class MockCommandExtractorInner : ICommandExtractor
    {
        public bool Called { get; set; }

        public bool IsExplicitError { get; set; }

        public bool IsExplicitNoCommand { get; set; }

        public bool IsExplicitNoCommandIdenitity { get; set; }

        public Task<CommandExtractorResult> ExtractAsync(CommandExtractorContext context)
        {
            Called = true;

            if (IsExplicitError)
            {
                throw new ErrorException("test_extractor_error", "test_extractor_error_desc");
            }
            else
            {
                if (IsExplicitNoCommand)
                {
                    // No error but no command
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    return Task.FromResult(new CommandExtractorResult(null, new CommandDescriptor("test_id", "test_name", "test_prefix", "desc")));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
                else if (IsExplicitNoCommandIdenitity)
                {
                    // No error but no command descriptor
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    return Task.FromResult(new CommandExtractorResult(new Command("test_id", "test_name", "desc"), null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
                else
                {
                    // all ok
                    return Task.FromResult(new CommandExtractorResult(new Command("test_id", "test_name", "desc"), new CommandDescriptor("test_id", "test_name", "test_prefix", "desc")));
                }
            }
        }
    }
}
