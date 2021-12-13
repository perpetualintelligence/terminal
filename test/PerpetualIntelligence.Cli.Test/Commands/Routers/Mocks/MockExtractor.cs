/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Shared.Infrastructure;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Routers.Mocks
{
    internal class MockExtractor : ICommandExtractor
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
                return Task.FromResult(OneImlxResult.NewError<CommandExtractorResult>("test_extractor_error", "test_extractor_error_desc"));
            }
            else
            {
                if (IsExplicitNoCommand)
                {
                    // No error but no command
                    return Task.FromResult(new CommandExtractorResult()
                    {
                        CommandIdentity = new CommandIdentity("test_id", "test_name", "test_prefix", null)
                    });
                }
                else if (IsExplicitNoCommandIdenitity)
                {
                    // No error but no command identity
                    return Task.FromResult(new CommandExtractorResult()
                    {
                        Command = new Command() { Id = "test_id", Name = "test_name" },
                    });
                }
                else
                {
                    // all ok
                    return Task.FromResult(new CommandExtractorResult()
                    {
                        Command = new Command() { Id = "test_id", Name = "test_name" },
                        CommandIdentity = new CommandIdentity("test_id", "test_name", "test_prefix", null)
                    });
                }
            }
        }
    }
}
