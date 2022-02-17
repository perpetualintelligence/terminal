/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Exceptions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// The default <see cref="ILicenseChecker"/> for all <c>cli</c> features.
    /// </summary>
    public sealed class LicenseChecker : ILicenseChecker
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public LicenseChecker(ILogger<LicenseChecker> logger)
        {
        }

        /// <summary>
        /// Checks the licensing context.
        /// </summary>
        /// <param name="context">The licensing context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<LicenseCheckerResult> CheckAsync(LicenseCheckerContext context)
        {
            // Make sure we have valid licenses to check
            await ExtractLicensesAsync();

            if (context.CommandDescriptor == null)
            {
                throw new ErrorException(Errors.InvalidLicense, "The command descriptor is missing in the request.");
            }

            List<License> validLicenses = new();
            if (context.CheckFeature.HasFlag(LicenseCheckerFeature.RootCommandLimit) && context.CommandDescriptor.IsRoot)
            {
                validLicenses.AddRange(await CheckCommandLimits(context.CommandDescriptor));
            }

            if (context.CheckFeature.HasFlag(LicenseCheckerFeature.CommandGroupLimit) && context.CommandDescriptor.IsGroup)
            {
                validLicenses.AddRange(await CheckCommandGroup(context.CommandDescriptor));
            }

            if (context.CheckFeature.HasFlag(LicenseCheckerFeature.CommandLimit))
            {
                validLicenses.AddRange(await CheckCommand(context.CommandDescriptor));
            }

            if (!validLicenses.Any())
            {
                throw new ErrorException(Errors.InvalidLicense, "The license check failed.");
            }

            return new LicenseCheckerResult(validLicenses.ToArray());
        }

        /// <summary>
        /// Checks the licensing context for root commands.
        /// </summary>
        /// <param name="commandDescriptor">The root command descriptor.</param>
        /// <exception cref="ErrorException"></exception>
        public Task<License[]> CheckCommandLimits(CommandDescriptor commandDescriptor)
        {
            // If any license does not have a limit then that means the subject has mac root commands.
            if (!soleLicense.RootCommandLimit.HasValue)
            {
                return Task.FromResult(new[] { soleLicense });
            }

            // Otherwise add command id and check the count. We add it first to ensure the duplicate command ids do not
            // bump up the count and valid duplicate ids should not fail.
            _commandIds.TryAdd(commandDescriptor.Id, true);

            int limit = soleLicense.RootCommandLimit.Value;
            if (_commandIds.Count <= limit)
            {
                // We have found a valid license within limit so reset the previous failed and return.
                return Task.FromResult(new[] { soleLicense });
            }

            throw new ErrorException(Errors.InvalidLicense, "The root command limit reached. max_limit={0} current={1}", soleLicense.RootCommandLimit, _commandIds.Count);
        }

        private Task<License[]> CheckCommand(CommandDescriptor commandDescriptor)
        {
            // If any license does not have a limit then that means the subject has mac root commands.
            if (!soleLicense.RootCommandLimit.HasValue)
            {
                return Task.FromResult(new[] { soleLicense });
            }

            // Otherwise add command id and check the count. We add it first to ensure the duplicate command ids do not
            // bump up the count and valid duplicate ids should not fail.
            _commandIds.TryAdd(commandDescriptor.Id, true);

            int limit = soleLicense.RootCommandLimit.Value;
            if (_commandIds.Count <= limit)
            {
                // We have found a valid license within limit so reset the previous failed and return.
                return Task.FromResult(new[] { soleLicense });
            }

            throw new ErrorException(Errors.InvalidLicense, "The root command limit reached. max_limit={0} current={1}", soleLicense.RootCommandLimit, _commandIds.Count);
        }

        private Task<IEnumerable<License>> CheckCommandGroup(CommandDescriptor commandDescriptor)
        {
            throw new System.NotImplementedException();
        }

        private async Task ExtractLicensesAsync()
        {
            var result = await licenseExtractor.ExtractAsync(new LicenseExtractorContext());

            // For now we only support 1 license
            if (result.Licenses.Count() != 1)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The license checker found multiple licenses. Please configure only 1 license");
            }

            soleLicense = result.Licenses.First();
        }

        private readonly ILicenseExtractor licenseExtractor;

        /// <summary>
        /// There is no concurrent HashSet so we use ConcurrentDictionary and put the minimal value (byte) which has no
        /// purpose for now. https://stackoverflow.com/questions/18922985/concurrent-hashsett-in-net-framework
        /// </summary>
        private ConcurrentDictionary<string, bool> _commandIds = new();

        // Can be duplicate
        private License? soleLicense = null;
    }
}
