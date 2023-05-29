/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Hosting.Mocks
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
