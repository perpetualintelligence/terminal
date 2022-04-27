/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Handlers;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The <c>pi-cli</c> handler options.
    /// </summary>
    public class HandlerOptions : Shared.Infrastructure.LoggingOptions
    {
        /// <summary>
        /// Reserved for future.
        /// </summary>
        public string? DataTypeHandler { get; set; }

        /// <summary>
        /// The hosting and routing error handler. Its value can be <c>default</c> or <c>custom</c>.
        /// </summary>
        /// <remarks>
        /// The command router receives an error or exception during the command routing, extraction, checker, or
        /// execution. On error, it forwards it to the <see cref="IErrorHandler"/> or <see cref="IExceptionHandler"/>.
        /// The <c>default</c> implementation will print the error information in the CLI terminal. Application authors
        /// can define a custom error handler to process and publish the error as per their needs.
        /// </remarks>
        /// <seealso cref="IErrorHandler"/>
        /// <seealso cref="Commands.Handlers.ErrorHandler"/>
        public string ErrorHandler { get; set; } = "default";

        /// <summary>
        /// The licensing handler. Its value can be <c>online</c>, <c>offline</c>, or <c>byol</c>. The <c>offline</c> or
        /// <c>byol</c> licensing are for future releases.
        /// </summary>
        public string LicenseHandler { get; set; } = "online";

        /// <summary>
        /// The hosting and routing dependency injection services. Its value can be <c>default</c> or <c>custom</c>. The
        /// <c>custom</c> service implementations are for future releases.
        /// </summary>
        public string ServiceHandler { get; set; } = "default";

        /// <summary>
        /// The command and argument store handler. Its value can be <c>in-memory</c>, <c>json</c> or <c>custom</c>. The
        /// <c>json</c> or <c>custom</c> store implementations are for future releases.
        /// </summary>
        public string StoreHandler { get; set; } = "in-memory";

        /// <summary>
        /// The hosting and routing Unicode command string handler. Its value can be <c>default</c>.
        /// </summary>
        /// <remarks>By default the <c>pi-cli</c> framework supports Unicode command strings.</remarks>
        public string UnicodeHandler { get; set; } = "default";
    }
}
