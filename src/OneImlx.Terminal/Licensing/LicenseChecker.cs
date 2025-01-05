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
                CommandCount = commandCount,
                InputCount = inputCount,
            };
        }

        private Task<License> CheckLimitsAsync(License license)
        {
            // Terminal limit
            if (terminalCount > license.Quota.TerminalLimit)
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The terminal limit exceeded. max_limit={0} current={1}", license.Quota.TerminalLimit, terminalCount);
            }

            // Redistribution limit TODO, how do we check redistribution in a native bounded context

            // Root command limit
            if (commandCount > license.Quota.CommandLimit)
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The command limit exceeded. max_limit={0} current={1}", license.Quota.CommandLimit, commandCount);
            }

            // Input limit
            if (inputCount > license.Quota.InputLimit)
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The input limit exceeded. max_limit={0} current={1}", license.Quota.InputLimit, inputCount);
            }

            // We have found a valid license within limit so reset the previous failed and return.
            return Task.FromResult(license);
        }

        private Task CheckOptionsAsync(License license)
        {
            // Follow the pricing http://localhost:8080/articles/pi-cli/pricing.html We drive all customization through
            // options and the License sets the allowed options. So here we don't need to check the license plan, just
            // check the options value with license value.
            LicenseQuota quota = license.Quota;

            // Strict Data Type
            if (!OptionsValid(quota.StrictDataType, terminalOptions.Checker.StrictValueType))
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The strict option value type is not allowed for your license plan.");
            }

            // Driver
            if (!OptionsValid(quota.Driver, terminalOptions.Driver.Enabled))
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The terminal driver option is not allowed for your license plan.");
            }

            // Integration
            if (!OptionsValid(quota.Integration, terminalOptions.Integration.Enabled))
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The terminal integration option is not allowed for your license plan.");
            }

            return Task.CompletedTask;
        }

        private async Task InitializeLockAsync(License license)
        {
            // Make sure the initialization is thread safe
            await semaphoreSlim.WaitAsync();
            try
            {
                // Make sure we don't double down on quota.
                if (initialized)
                {
                    return;
                }

                // All quota are per terminal. The terminal id is the authorized app id.
                terminalCount = 1;

                var commandDescriptors = await commandStore.AllAsync();
                foreach (var kvpCmd in commandDescriptors)
                {
                    // Commands
                    commandCount++;

                    // Arguments
                    if (kvpCmd.Value.ArgumentDescriptors != null)
                    {
                        inputCount += kvpCmd.Value.ArgumentDescriptors.Count;
                    }

                    // Options
                    if (kvpCmd.Value.OptionDescriptors != null)
                    {
                        inputCount += kvpCmd.Value.OptionDescriptors.Count;
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
        private long commandCount;
        private bool initialized;
        private long inputCount;
        private int terminalCount;
    }
}
