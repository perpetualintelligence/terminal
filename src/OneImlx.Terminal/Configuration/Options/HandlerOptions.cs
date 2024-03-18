/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Configuration.Options
{
    /// <summary>
    /// The handler options.
    /// </summary>
    public sealed class HandlerOptions
    {
        /// <summary>
        /// The license handler. Its value can be <c>online</c>, <c>offline</c>, or <c>onpremise</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="TerminalHandlers.OnlineLicenseHandler"/> handler checks your license key online. Your terminal needs public network access during startup.
        /// </para>
        /// <para>
        ///  <see cref="TerminalHandlers.OfflineLicenseHandler"/> handler checks your license key offline. Your terminal does not need network access during startup, but requires you to periodically update the license key from your secured network.
        /// </para>
        /// <para>
        /// <see cref="TerminalHandlers.OnPremiseLicenseHandler"/> handler checks the license only when your terminal is attached to the debugger on the local developer node. Your terminal does not check license key on your on-premise production or released setup.
        /// </para>
        /// </remarks>
        public string LicenseHandler { get; set; } = "online";
    }
}