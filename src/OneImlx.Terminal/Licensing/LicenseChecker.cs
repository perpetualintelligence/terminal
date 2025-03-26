/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Shared;
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
            if (!OptionsValid(quota.DataType, terminalOptions.Checker.ValueDataType))
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The strict option value type is not allowed for your license plan.");
            }

            // Driver
            if (!OptionsValid(quota.Driver, terminalOptions.Driver.Enabled))
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The terminal driver option is not allowed for your license plan.");
            }

            // Integration
            if (!OptionsValid(quota.Dynamics, terminalOptions.Dynamics.Enabled))
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The terminal dynamics option is not allowed for your license plan.");
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
