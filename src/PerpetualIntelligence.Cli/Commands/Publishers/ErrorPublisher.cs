/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Publishers
{
    /// <summary>
    /// The default <see cref="IErrorPublisher"/> to publish an <see cref="Error"/>.
    /// </summary>
    public class ErrorPublisher : IErrorPublisher
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public ErrorPublisher(CliOptions options, ILogger<ExceptionPublisher> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Publish the <see cref="Error"/> asynchronously
        /// </summary>
        /// <param name="context">The error to publish.</param>
        /// <returns>The string representation.</returns>
        public Task PublishAsync(ErrorPublisherContext context)
        {
            logger.FormatAndLog(LogLevel.Error, options.Logging, context.Error.ErrorDescription ?? context.Error.ErrorCode, context.Error.Args ?? Array.Empty<object?>());
            return Task.CompletedTask;
        }

        private ILogger<ExceptionPublisher> logger;
        private CliOptions options;
    }
}
