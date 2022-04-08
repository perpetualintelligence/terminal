/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands;

using PerpetualIntelligence.Shared.Exceptions;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        public Task<LicenseCheckerResult> CheckAsync(LicenseCheckerContext context)
        {
            if (context.CommandDescriptor == null)
            {
                throw new ErrorException(Errors.InvalidLicense, "The command descriptor is missing in the request.");
            }

            return Task.FromResult(new LicenseCheckerResult(context.License));
        }

        /// <summary>
        /// Checks the licensing context for root commands.
        /// </summary>
        /// <param name="context">The root command descriptor.</param>
        /// <exception cref="ErrorException"></exception>
        public Task<License> CheckCommandLimits(LicenseCheckerContext context)
        {
            // If any license does not have a limit then that means the subject has max root commands.
            if (!context.License.Limits.SubCommandLimit.HasValue)
            {
                return Task.FromResult(context.License);
            }

            // Otherwise add command id and check the count. We add it first to ensure the duplicate command ids do not
            // bump up the count and valid duplicate ids should not fail.
            _commandIds.TryAdd(context.CommandDescriptor.Id, true);

            long limit = context.License.Limits.SubCommandLimit.Value;
            if (_commandIds.Count <= limit)
            {
                // We have found a valid license within limit so reset the previous failed and return.
                return Task.FromResult(context.License);
            }

            throw new ErrorException(Errors.InvalidLicense, "The root command limit reached. max_limit={0} current={1}", context.License.Limits.RootCommandLimit, _commandIds.Count);
        }

        private Task<IEnumerable<License>> CheckCommandGroup(CommandDescriptor commandDescriptor)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// There is no concurrent HashSet so we use ConcurrentDictionary and put the minimal value (bit) which has no
        /// purpose for now. https://stackoverflow.com/questions/18922985/concurrent-hashsett-in-net-framework
        /// </summary>
        private ConcurrentDictionary<string, bool> _commandIds = new();
    }
}
