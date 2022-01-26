/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test.Services;
using System;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    [TestClass]
    public class ExtractorResultTests
    {
        [TestMethod]
        public void ArgumentCheckerResultNullMappedTypeShouldThrow()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            TestHelper.AssertThrowsWithMessage<ArgumentNullException>(() => new ArgumentCheckerResult(null), "Value cannot be null. (Parameter 'mappedType')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }
    }
}
