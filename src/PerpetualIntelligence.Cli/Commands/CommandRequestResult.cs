/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The default command request result with no response.
    /// </summary>
    public class CommandRequestResult : OneImlxResult, IPublisher<CommandRequestContext>
    {
        /// <summary>
        /// The checked command.
        /// </summary>
        public Command? Command { get; set; }

        /// <summary>
        /// Determines whether the result publishes a response for the requesting party (RP).
        /// </summary>
        public virtual bool PublishesResponse
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// The command runner.
        /// </summary>
        public ICommandRunner? Runner { get; set; }

        /// <inheritdoc/>
        public async Task ProcessResultAsync(CommandRequestContext context)
        {
            // DO we need to check the Command and command string here ?
            if (Command == null || !Command.Checked)
            {
                // FOMAC: why throw, we should change the syntax to return result here ?
                throw new CheckerException("The command is null or not checked.");
            }

            if (Runner == null)
            {
                // FOMAC: why throw, we should change the syntax to return result here ?
                throw new CheckerException("The command runner is not set.");
            }

            // Now run the command
            // FOMAC: we need to return the result not set it on context.
            context.RunnerResult = await Runner.RunAsync(new CommandRunnerContext(Command));
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if <see cref="PublishesResponse"/> is <c>false</c>.
        /// </summary>
        public void ThrowIfNotPublishesResponse()
        {
            if (!PublishesResponse)
            {
                throw new InvalidOperationException($"The '{this.GetType().FullName}' must publish response.");
            }
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if <see cref="PublishesResponse"/> is <c>true</c>.
        /// </summary>
        public void ThrowIfPublishesResponse()
        {
            if (PublishesResponse)
            {
                throw new InvalidOperationException($"The '{this.GetType().FullName}' cannot publish response.");
            }
        }
    }
}
