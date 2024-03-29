﻿/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Checkers;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    public class MockOptionChecker : IOptionChecker
    {
        public bool Called { get; set; }

        public Task<OptionCheckerResult> CheckOptionAsync(OptionCheckerContext context)
        {
            Called = true;
            return Task.FromResult(new OptionCheckerResult(typeof(string)));
        }
    }
}
