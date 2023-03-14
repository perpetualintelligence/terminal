/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Hosting.Mocks
{
    /// <summary>
    /// </summary>
    public class MockOptionsChecker : IConfigurationOptionsChecker
    {
        public ValueTuple<int, bool> CheckOptionsCalled { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task CheckAsync(CliOptions context)
        {
            CheckOptionsCalled = new(MockCliHostedServiceStaticCounter.Increment(), true);
            return Task.CompletedTask;
        }
    }
}
