/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Handlers;
using System.Threading;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The <c>pi-cli</c> hosting options.
    /// </summary>
    public class HostingOptions : Shared.Infrastructure.LoggingOptions
    {
        /// <summary>
        /// The hosting and routing error handler. Its value can be <c>default</c> or <c>custom</c>.
        /// </summary>
        /// <remarks>
        /// The command router receives an error or exception during the command routing, extraction, checker, or
        /// execution. On error it forwards it to the <see cref="IErrorHandler"/>. The
        /// <c>default</c><see cref="IErrorHandler"/> will print the error information in the CLI terminal.
        /// Application authors can define a custom error handler to process and publish the error as per the
        /// application requirement.
        /// </remarks>
        /// <seealso cref="IErrorHandler"/>
        /// <seealso cref="Commands.Handlers.ErrorHandler"/>
        public string ErrorHandler { get; set; } = "default";

        /// <summary>
        /// Defines the hosting and routing service implementation. Defaults to <c>default</c> service implementation.
        /// </summary>
        public string Service { get; set; } = "default";

        /// <summary>
        /// Defines the hosting and routing store. Defaults to <c>in-memory</c> store implementation.
        /// </summary>
        public string Store { get; set; } = "in-memory";

        /// <summary>
        /// Defines the hosting and routing service implementation. Defaults to <c>default</c> service implementation.
        /// </summary>
        public string UnicodeHandler { get; set; } = "default";
    }
}
