/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    [TestClass]
    public class CommandCheckerTests : OneImlxLogTest
    {
        public CommandCheckerTests() : base(TestLogger.Create<CommandCheckerTests>())
        {
        }

        protected override void OnTestInitialize()
        {
            options = MockCliOptions.New();
            checker = new CommandChecker(options, TestLogger.Create<CommandChecker>());
        }

        private CommandChecker checker = null!;
        private CliOptions options = MockCliOptions.New();
    }
}
