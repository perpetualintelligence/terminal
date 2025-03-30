/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Infrastructure;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;
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
        public LicenseChecker(
            ITerminalCommandStore commandStore,
            ITerminalTextHandler textHandler,
            TerminalOptions terminalOptions,
            ILogger<LicenseChecker> logger)
        {
            this.commandStore = commandStore;
            this.textHandler = textHandler;
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
            try
            {
                logger.LogDebug("Check license. plan={0} usage={1} subject={2} tenant={3}", license.Plan, license.Usage, license.Claims.Subject, license.Claims.TenantId);

                // Initialize if needed
                await InitializeLockAsync(license);

                // Check Limits
                await CheckLimitsAsync(license);

                // Check Options
                await CheckOptionsAsync(license);
            }
            catch (Exception ex)
            {
                if (ex is TerminalException tex)
                {
                    license.SetFailed(tex.Error);
                }
                else
                {
                    license.SetFailed(new Error(TerminalErrors.ServerError, ex.Message));
                }

                throw;
            }

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
                throw new TerminalException(TerminalErrors.InvalidLicense, "The terminal driver is not allowed for your license plan.");
            }

            // Dynamics
            if (!OptionsValid(quota.Dynamics, terminalOptions.Dynamics.Enabled))
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The terminal dynamics is not allowed for your license plan.");
            }

            // Authentications
            if (!OptionsValid(quota.Authentications, terminalOptions.Authentication.Provider))
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, $"The terminal authentication `{terminalOptions.Authentication.Provider}` is not allowed for your license plan.");
            }

            // Deployments
            if (!OptionsValid(quota.Deployments, terminalOptions.Licensing.Deployment))
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, $"The terminal deployment `{terminalOptions.Licensing.Deployment}` is not allowed for your license plan.");
            }

            // Stores
            string acutualStore = commandStore.GetType().IsAssignableFrom(typeof(TerminalInMemoryCommandStore)) ? "memory" : "custom";
            if (!OptionsValid(quota.Stores, acutualStore))
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, $"The terminal store `{acutualStore}` is not allowed for your license plan.");
            }

            // Encodings
            string actualEncoding = GetTextHandlerEncoding(textHandler.Encoding);
            if (!OptionsValid(quota.Encodings, actualEncoding))
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, $"The terminal text handler `{actualEncoding}` is not allowed for your license plan.");
            }

            // Routers
            if (!OptionsValid(quota.Routers, terminalOptions.Router.Name))
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, $"The terminal router `{terminalOptions.Router.Name}` is not allowed for your license plan.");
            }

            return Task.CompletedTask;
        }

        private string GetTextHandlerEncoding(Encoding encoding)
        {
            if (encoding.Equals(Encoding.ASCII))
            {
                return "ascii";
            }

            if (encoding.Equals(Encoding.UTF8))
            {
                return "utf8";
            }

            if (encoding.Equals(Encoding.Unicode))
            {
                return "utf16";
            }

            if (encoding.Equals(Encoding.UTF32))
            {
                return "utf32";
            }

            return "none";
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

        private bool OptionsValid(IEnumerable<string> allowed, string? actual)
        {
            if (allowed.Contains(actual, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private bool OptionsValid(bool allowed, bool actual)
        {
            // An allowed value can be true or false, actual value cannot be true if allowed value is false.
            if (allowed == false && actual == true)
            {
                return false;
            }

            return true;
        }

        private readonly ITerminalCommandStore commandStore;
        private readonly ILogger<LicenseChecker> logger;
        private readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        private readonly TerminalOptions terminalOptions;
        private readonly ITerminalTextHandler textHandler;
        private long commandCount;
        private bool initialized;
        private long inputCount;
        private int terminalCount;
    }
}
