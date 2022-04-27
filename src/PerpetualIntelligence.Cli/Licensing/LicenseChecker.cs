/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// The default <see cref="ILicenseChecker"/> for all <c>pi-cli</c> features.
    /// </summary>
    public sealed class LicenseChecker : ILicenseChecker
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public LicenseChecker(IEnumerable<CommandDescriptor> commandDescriptors, CliOptions cliOptions, ILogger<LicenseChecker> logger)
        {
            this.commandDescriptors = commandDescriptors;
            this.cliOptions = cliOptions;
            this.logger = logger;
        }

        /// <summary>
        /// Determines whether the checker is initialized.
        /// </summary>
        public bool Initialized { get => initialized; }

        /// <summary>
        /// Checks the licensing context.
        /// </summary>
        /// <param name="context">The licensing context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<LicenseCheckerResult> CheckAsync(LicenseCheckerContext context)
        {
            // Initialize if needed
            InitializeLock(context.License);

            // Check Limits
            await CheckLimitsAsync(context);

            // Check Limits
            await CheckOptionsAsync(context);

            return new LicenseCheckerResult(context.License)
            {
                TerminalCount = terminalCount,
                RootCommandCount = rootCommandCount,
                CommandGroupCount = commandGroupCount,
                SubCommandCount = subCommandCount,
                ArgumentCount = argumentCount,
            };
        }

        private Task<License> CheckLimitsAsync(LicenseCheckerContext context)
        {
            // Terminal limit
            if (terminalCount > context.License.Limits.TerminalLimit)
            {
                throw new ErrorException(Errors.InvalidLicense, "The terminal limit exceeded. max_limit={0} current={1}", context.License.Limits.TerminalLimit, terminalCount);
            }

            // Redistribution limit TODO, how do we check redistribution in a native bounded context

            // Root command limit
            if (rootCommandCount > context.License.Limits.RootCommandLimit)
            {
                throw new ErrorException(Errors.InvalidLicense, "The root command limit exceeded. max_limit={0} current={1}", context.License.Limits.RootCommandLimit, rootCommandCount);
            }

            // Command group limit
            if (commandGroupCount > context.License.Limits.GroupedCommandLimit)
            {
                throw new ErrorException(Errors.InvalidLicense, "The command group limit exceeded. max_limit={0} current={1}", context.License.Limits.GroupedCommandLimit, commandGroupCount);
            }

            // Command group limit
            if (subCommandCount > context.License.Limits.SubCommandLimit)
            {
                throw new ErrorException(Errors.InvalidLicense, "The sub command limit exceeded. max_limit={0} current={1}", context.License.Limits.SubCommandLimit, subCommandCount);
            }

            // Argument limit
            if (argumentCount > context.License.Limits.ArgumentLimit)
            {
                throw new ErrorException(Errors.InvalidLicense, "The argument limit exceeded. max_limit={0} current={1}", context.License.Limits.ArgumentLimit, argumentCount);
            }

            // We have found a valid license within limit so reset the previous failed and return.
            return Task.FromResult(context.License);
        }

        private Task CheckOptionsAsync(LicenseCheckerContext context)
        {
            // Follow the pricing http://localhost:8080/articles/pi-cli/pricing.html We drive all customization through
            // options and the License sets the allowed options. So here we don't need to check the license plan, just
            // check the options value with license value.
            LicenseLimits limits = context.License.Limits;

            // Argument alias
            if (!OptionsValid(limits.ArgumentAlias, cliOptions.Extractor.ArgumentAlias))
            {
                throw new ErrorException(Errors.InvalidLicense, "The configured argument alias option is not allowed for your license edition.");
            }

            // Default arguments
            if (!OptionsValid(limits.DefaultArgument, cliOptions.Extractor.DefaultArgument))
            {
                throw new ErrorException(Errors.InvalidLicense, "The configured default argument option is not allowed for your license edition.");
            }

            // Default argument value
            if (!OptionsValid(limits.DefaultArgumentValue, cliOptions.Extractor.DefaultArgumentValue))
            {
                throw new ErrorException(Errors.InvalidLicense, "The configured default argument value option is not allowed for your license edition.");
            }

            // Strict Data Type
            if (!OptionsValid(limits.StrictDataType, cliOptions.Checker.StrictArgumentValueType))
            {
                throw new ErrorException(Errors.InvalidLicense, "The configured strict argument value type option is not allowed for your license edition.");
            }

            // Date Type handler, null allowed for data type handler.
            if (!OptionsValid(limits.DataTypeHandlers, cliOptions.Handler.DataTypeHandler, allowNullActual: true))
            {
                throw new ErrorException(Errors.InvalidLicense, "The configured data-type handler is not allowed for your license edition. datatype_handler={0}", cliOptions.Handler.DataTypeHandler);
            }

            // Unicode handler
            if (!OptionsValid(limits.UnicodeHandlers, cliOptions.Handler.TextHandler, allowNullActual: false))
            {
                throw new ErrorException(Errors.InvalidLicense, "The configured text handler is not allowed for your license edition.text_handler={0}", cliOptions.Handler.TextHandler);
            }

            // Error handler
            if (!OptionsValid(limits.ErrorHandlers, cliOptions.Handler.ErrorHandler, allowNullActual: false))
            {
                throw new ErrorException(Errors.InvalidLicense, "The configured error handler is not allowed for your license edition. error_handler={0}", cliOptions.Handler.ErrorHandler);
            }

            // Store handler
            if (!OptionsValid(limits.StoreHandlers, cliOptions.Handler.StoreHandler, allowNullActual: false))
            {
                throw new ErrorException(Errors.InvalidLicense, "The configured store handler is not allowed for your license edition. store_handler={0}", cliOptions.Handler.StoreHandler);
            }

            // Service handler
            if (!OptionsValid(limits.ServiceHandlers, cliOptions.Handler.ServiceHandler, allowNullActual: false))
            {
                throw new ErrorException(Errors.InvalidLicense, "The configured service handler is not allowed for your license edition. service_handler={0}", cliOptions.Handler.ServiceHandler);
            }

            // License handler
            if (!OptionsValid(limits.LicenseHandlers, cliOptions.Handler.LicenseHandler, allowNullActual: false))
            {
                throw new ErrorException(Errors.InvalidLicense, "The configured license handler is not allowed for your license edition. license_handler={0}", cliOptions.Handler.LicenseHandler);
            }

            return Task.CompletedTask;
        }

        private void InitializeLock(License license)
        {
            // Make sure the initialization is thread safe
            lock (lockObject)
            {
                // Make sure we down double down on limits.
                if (initialized)
                {
                    return;
                }

                // TODO, how do we sync terminal count across apps
                terminalCount = 1;

                foreach (CommandDescriptor cmd in commandDescriptors)
                {
                    // Register the commands
                    if (cmd.IsRoot)
                    {
                        rootCommandCount += 1;
                    }
                    else if (cmd.IsGroup)
                    {
                        commandGroupCount += 1;
                    }

                    // All are commands
                    subCommandCount += 1;

                    // For now we only care about argument count.
                    if (cmd.ArgumentDescriptors != null)
                    {
                        argumentCount += 1;
                    }
                }

                initialized = true;
            }
        }

        private bool OptionsValid(string[]? allowed, string? actual, bool? allowNullActual = true)
        {
            // The actual value can be null, it represents that the user has not configured the feature.
            if (actual == null)
            {
                if (allowNullActual.GetValueOrDefault())
                {
                    return true;
                }
                else
                {
                    // Some options have non null default value.
                    return false;
                }
            }

            // If expected is null then actual cannot be non null
            if (allowed == null)
            {
                return false;
            }

            return allowed.Contains(actual);
        }

        private bool OptionsValid(bool? allowed, bool? actual)
        {
            // An expected value can be true or false, actual value cannot be true if expected value is false.
            if (allowed.GetValueOrDefault() == false && actual.GetValueOrDefault() == true)
            {
                return false;
            }

            return true;
        }

        private readonly CliOptions cliOptions;
        private readonly IEnumerable<CommandDescriptor> commandDescriptors;
        private readonly ILogger<LicenseChecker> logger;

        private long argumentCount;
        private long commandGroupCount;
        private bool initialized;
        private object lockObject = new();
        private long rootCommandCount;
        private long subCommandCount;
        private int terminalCount;
    }
}
