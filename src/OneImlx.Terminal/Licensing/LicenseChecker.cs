/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Stores;

namespace OneImlx.Terminal.Licensing
{
    /// <summary>
    /// The default <see cref="ILicenseChecker"/> for all features.
    /// </summary>
    public sealed class LicenseChecker : ILicenseChecker
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public LicenseChecker(ITerminalCommandStore commandStore, TerminalOptions terminalOptions, ILogger<LicenseChecker> logger)
        {
            this.commandStore = commandStore;
            this.terminalOptions = terminalOptions;
            this.logger = logger;
        }

        /// <summary>
        /// Determines whether the checker is initialized.
        /// </summary>
        public bool Initialized { get => initialized; }

        /// <inheritdoc/>
        public async Task<LicenseCheckerResult> CheckLicenseAsync(License license)
        {
            logger.LogDebug("Check license. plan={0} usage={1} subject={2} tenant={3}", license.Plan, license.Usage, license.Claims.Subject, license.Claims.TenantId);

            // Initialize if needed
            await InitializeLockAsync(license);

            // Check Limits
            await CheckLimitsAsync(license);

            // Check Options
            await CheckOptionsAsync(license);

            return new LicenseCheckerResult(license)
            {
                TerminalCount = terminalCount,
                RootCommandCount = rootCommandCount,
                CommandGroupCount = commandGroupCount,
                SubCommandCount = subCommandCount,
                NativeCommandCount = nativeCommandCount,
                OptionCount = optionCount,
            };
        }

        private Task<License> CheckLimitsAsync(License license)
        {
            // Terminal limit
            if (terminalCount > license.Limits.TerminalLimit)
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The terminal limit exceeded. max_limit={0} current={1}", license.Limits.TerminalLimit, terminalCount);
            }

            // Redistribution limit TODO, how do we check redistribution in a native bounded context

            // Root command limit
            if (rootCommandCount > license.Limits.RootCommandLimit)
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The root command limit exceeded. max_limit={0} current={1}", license.Limits.RootCommandLimit, rootCommandCount);
            }

            // grouped command limit
            if (commandGroupCount > license.Limits.GroupedCommandLimit)
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The grouped command limit exceeded. max_limit={0} current={1}", license.Limits.GroupedCommandLimit, commandGroupCount);
            }

            // grouped command limit
            if (subCommandCount > license.Limits.SubCommandLimit)
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The sub command limit exceeded. max_limit={0} current={1}", license.Limits.SubCommandLimit, subCommandCount);
            }

            // Option limit
            if (optionCount > license.Limits.OptionLimit)
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The option limit exceeded. max_limit={0} current={1}", license.Limits.OptionLimit, optionCount);
            }

            // We have found a valid license within limit so reset the previous failed and return.
            return Task.FromResult(license);
        }

        private Task CheckOptionsAsync(License license)
        {
            // Follow the pricing http://localhost:8080/articles/pi-cli/pricing.html We drive all customization through
            // options and the License sets the allowed options. So here we don't need to check the license plan, just
            // check the options value with license value.
            LicenseLimits limits = license.Limits;

            // Strict Data Type
            if (!OptionsValid(limits.StrictDataType, terminalOptions.Checker.StrictValueType))
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The configured strict option value type is not allowed for your license plan.");
            }

            // Authentication
            if (!OptionsValid(limits.Authentication, terminalOptions.Authentication.Enabled))
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The configured terminal authentication is not allowed for your license plan.");
            }

            return Task.CompletedTask;
        }

        private async Task InitializeLockAsync(License license)
        {
            // Make sure the initialization is thread safe
            await semaphoreSlim.WaitAsync();
            try
            {
                // Make sure we don't double down on limits.
                if (initialized)
                {
                    return;
                }

                // TODO, how do we sync terminal count across apps
                terminalCount = 1;

                var commandDescriptors = await commandStore.AllAsync();
                foreach (var kvpCmd in commandDescriptors)
                {
                    // Register the commands
                    if (kvpCmd.Value.Type == Commands.CommandType.RootCommand)
                    {
                        rootCommandCount += 1;
                    }
                    else if (kvpCmd.Value.Type == Commands.CommandType.GroupCommand)
                    {
                        commandGroupCount += 1;
                    }
                    else if (kvpCmd.Value.Type == Commands.CommandType.SubCommand)
                    {
                        subCommandCount += 1;
                    }
                    else if (kvpCmd.Value.Type == Commands.CommandType.NativeCommand)
                    {
                        nativeCommandCount += 1;
                    }
                    else
                    {
                        throw new TerminalException(TerminalErrors.InvalidCommand, "The command type is not supported. type={0}", kvpCmd.Value.Type);
                    }

                    // For now we only care about option count.
                    if (kvpCmd.Value.OptionDescriptors != null)
                    {
                        optionCount += kvpCmd.Value.OptionDescriptors.Count;
                    }
                }

                initialized = true;
            }
            finally
            {
                semaphoreSlim.Release();
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

        private readonly ITerminalCommandStore commandStore;
        private readonly ILogger<LicenseChecker> logger;
        private readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        private readonly TerminalOptions terminalOptions;
        private long commandGroupCount;
        private bool initialized;
        private long nativeCommandCount;
        private long optionCount;
        private long rootCommandCount;
        private long subCommandCount;
        private int terminalCount;
    }
}
