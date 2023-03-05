/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;

namespace PerpetualIntelligence.Cli.Runtime
{
    /// <summary>
    /// An abstraction to log messages to the terminal.
    /// </summary>
    /// <remarks>
    /// Most terminals show helpful messages to the user as it runs a command. The <see cref="ITerminalLogger"/> abstraction allows
    /// the applications to define and display these terminal messages. It also allows the application to define message format is, e.g., a standard
    /// console terminal message or a custom terminal UX message. It implements Microsoft's logging pattern. However, it is separate from the standard
    /// <see cref="ILogger"/>. Developers use the standard logger to generate application and event logs, while the terminal
    /// logger always displays the messages to the user.
    /// </remarks>
    public interface ITerminalLogger : ILogger
    {
    }
}