/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Shared.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Cli.Runtime
{
    [Collection("Sequential")]
    public class TerminalConsoleLoggerTests : IAsyncLifetime
    {
        public TerminalConsoleLoggerTests()
        {
            originalOut = Console.Out;
            stringWriter = new StringWriter();
            consoleLogger = new TerminalConsoleLogger("test", MockCliOptions.NewOptions());
        }

        [Fact]
        public void Scopes_Should_Indent_Correctly()
        {
            consoleLogger.LogInformation("Test message1");
            consoleLogger.LogInformation("Test message2");
            consoleLogger.LogInformation("Test message3");

            using (consoleLogger.BeginScope("Test1"))
            {
                consoleLogger.LogInformation("Test message11");
                consoleLogger.LogInformation("Test message12");
                consoleLogger.LogInformation("Test message13");
            }

            using (consoleLogger.BeginScope("Test2"))
            {
                consoleLogger.LogInformation("Test message21");
                consoleLogger.LogInformation("Test message22");
                consoleLogger.LogInformation("Test message23");

                using (consoleLogger.BeginScope("Test3"))
                {
                    consoleLogger.LogInformation("Test message31");
                    consoleLogger.LogInformation("Test message32");
                    consoleLogger.LogInformation("Test message33");

                    using (consoleLogger.BeginScope("Test4"))
                    {
                        consoleLogger.LogInformation("Test message41");
                        consoleLogger.LogInformation("Test message42");
                        consoleLogger.LogInformation("Test message43");
                    }

                    using (consoleLogger.BeginScope("Test5"))
                    {
                        consoleLogger.LogInformation("Test message51");
                        consoleLogger.LogInformation("Test message52");
                        consoleLogger.LogInformation("Test message53");
                    }
                }

                using (consoleLogger.BeginScope("Test6"))
                {
                    consoleLogger.LogInformation("Test message61");
                    consoleLogger.LogInformation("Test message62");
                    consoleLogger.LogInformation("Test message63");
                }
            }

            using (consoleLogger.BeginScope("Test7"))
            {
                consoleLogger.LogInformation("Test message71");
                consoleLogger.LogInformation("Test message72");
                consoleLogger.LogInformation("Test message73");
            }

            consoleLogger.LogInformation("Test message8");
            consoleLogger.LogInformation("Test message9");
            consoleLogger.LogInformation("Test message10");

            string[] expectedSplit = stringWriter.ToString().SplitByNewline();
            expectedSplit.Length.Should().Be(28);

            expectedSplit[0].Should().Be("Test message1");
            expectedSplit[1].Should().Be("Test message2");
            expectedSplit[2].Should().Be("Test message3");

            expectedSplit[3].Should().Be("    Test message11");
            expectedSplit[4].Should().Be("    Test message12");
            expectedSplit[5].Should().Be("    Test message13");

            expectedSplit[6].Should().Be("    Test message21");
            expectedSplit[7].Should().Be("    Test message22");
            expectedSplit[8].Should().Be("    Test message23");

            expectedSplit[9].Should().Be("        Test message31");
            expectedSplit[10].Should().Be("        Test message32");
            expectedSplit[11].Should().Be("        Test message33");

            expectedSplit[12].Should().Be("            Test message41");
            expectedSplit[13].Should().Be("            Test message42");
            expectedSplit[14].Should().Be("            Test message43");

            expectedSplit[15].Should().Be("            Test message51");
            expectedSplit[16].Should().Be("            Test message52");
            expectedSplit[17].Should().Be("            Test message53");

            expectedSplit[18].Should().Be("        Test message61");
            expectedSplit[19].Should().Be("        Test message62");
            expectedSplit[20].Should().Be("        Test message63");

            expectedSplit[21].Should().Be("    Test message71");
            expectedSplit[22].Should().Be("    Test message72");
            expectedSplit[23].Should().Be("    Test message73");

            expectedSplit[24].Should().Be("Test message8");
            expectedSplit[25].Should().Be("Test message9");
            expectedSplit[26].Should().Be("Test message10");

            expectedSplit[27].Should().BeEmpty();
        }

        public Task InitializeAsync()
        {
            Console.SetOut(stringWriter);
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            stringWriter.Dispose();
            Console.SetOut(originalOut);
            return Task.CompletedTask;
        }

        private readonly TerminalConsoleLogger consoleLogger;
        private readonly TextWriter originalOut;
        private readonly StringWriter stringWriter;
    }
}