/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Oidc;
using PerpetualIntelligence.Shared.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The default command extractor.
    /// </summary>
    public class StringCommandExtractor : ICommandExtractor
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandLookup">The command lookup.</param>
        /// <param name="argumentsExtractor">The arguments extractor.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public StringCommandExtractor(ICommandIdentityStore commandLookup, IArgumentsExtractor argumentsExtractor, CliOptions options, ILogger<StringCommandExtractor> logger)
        {
            this.commandLookup = commandLookup ?? throw new ArgumentNullException(nameof(commandLookup));
            this.argumentsExtractor = argumentsExtractor ?? throw new ArgumentNullException(nameof(argumentsExtractor));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<CommandExtractorResult> ExtractAsync(CommandExtractorContext context)
        {
            CommandExtractorResult result = new();

            // Check if command string is empty
            if (string.IsNullOrWhiteSpace(context.Command))
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command string is missing in the request.");
                result.SetError(Errors.InvalidRequest, errorDesc);
                return result;
            }

            // First Split by space
            string[] args = context.Command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Make sure the command is valid Command name (non grouped)
            string commandName = args[0];
            Shared.Infrastructure.OneImlxTryResult<CommandIdentity> commandResult = await commandLookup.TryFindByNameAsync(commandName);
            if (commandResult.IsError)
            {
                result.SetError(commandResult.TryError);
                return result;
            }

            // Get the arguments string
            string argString = context.Command.TrimStart(commandName.ToArray());

            // Extract arguments
            ArgumentsExtractorResult argResult = await argumentsExtractor.ExtractAsync(new ArgumentsExtractorContext(commandResult.Result, argString));
            if (argResult.IsError)
            {
                result.SyncError(argResult);
                return result;
            }

            // OK, return the extracted command object.
            return new CommandExtractorResult()
            {
                Command = new Command()
                {
                    Name = commandName,
                    Arguments = argResult.Arguments
                }
            };
        }

        private readonly IArgumentsExtractor argumentsExtractor;
        private readonly ICommandIdentityStore commandLookup;
        private readonly ILogger<StringCommandExtractor> logger;
        private readonly CliOptions options;
    }
}
