/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Hosting.Mocks
{
    public static class MockTerminalHostedServiceStaticCounter
    {
        public static int Increment()
        {
            Counter++;
            return Counter;
        }

        public static void Restart()
        {
            Counter = 0;
        }

        private static int Counter { get; set; }
    }
}
