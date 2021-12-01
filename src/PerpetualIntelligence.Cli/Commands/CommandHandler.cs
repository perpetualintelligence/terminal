/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The default command request handler.
    /// </summary>
    public class CommandHandler : ICommandRequestHandler
    {
        /// <summary>
        /// Initialize a news instance.
        /// </summary>
        public CommandHandler(ICommandExtractor commandExtractor, ICommandChecker commandChecker, CliOptions options, ILogger<CommandHandler> logger)
        {
            this.commandExtractor = commandExtractor ?? throw new ArgumentNullException(nameof(commandExtractor));
            this.commandChecker = commandChecker ?? throw new ArgumentNullException(nameof(commandChecker));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<CommandResult> HandleRequestAsync(CommandContext context)
        {
            CommandResult result = new();

            // Extract the command
            CommandExtractorResult extractorResult = await commandExtractor.ExtractAsync(new CommandExtractorContext(context.CommandString));
            if (extractorResult.IsError)
            {
                result.SyncError(extractorResult);
                return result;
            }

            // Check the command
            Command command = extractorResult.Command!;
            CommandCheckerResult checkerResult = await commandChecker.CheckAsync(new CommandCheckerContext(command));
            if (extractorResult.IsError)
            {
                result.SyncError(extractorResult);
                return result;
            }

            // Mark the command as checked
            command.Checked = true;

            // Return the result to process it further.
            result.Command = command;
            return result;
        }

        private readonly ICommandChecker commandChecker;
        private readonly ICommandExtractor commandExtractor;
        private readonly ILogger<CommandHandler> logger;
        private readonly CliOptions options;
    }
}
