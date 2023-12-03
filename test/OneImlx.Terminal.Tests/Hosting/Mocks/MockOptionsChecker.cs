/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Hosting.Mocks
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
        public Task CheckAsync(TerminalOptions context)
        {
            CheckOptionsCalled = new(MockTerminalHostedServiceStaticCounter.Increment(), true);
            return Task.CompletedTask;
        }
    }
}
