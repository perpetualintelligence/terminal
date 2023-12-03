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
        /// The license handler. Its value can be <c>online-license</c>, <c>offline-license</c>, or <c>onpremise-license</c>.
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
        public string LicenseHandler { get; set; } = "online-license";

        /// <summary>
        /// The hosting and routing dependency injection services. Its value can be <c>default</c> or <c>custom</c>. The
        /// <c>custom</c> is reserved for future releases.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>default</c> handler provides default DI service implementations for command router, parser, handler,
        /// mapper, checker and invoking runner.
        /// </para>
        /// <para><c>custom</c> handler allows application authors to define a custom DI services implementations.</para>
        /// </remarks>
        public string ServiceHandler { get; set; } = "default";

        /// <summary>
        /// The command and option store handler. Its value can be <c>in-memory</c>, <c>json</c> or <c>custom</c>. The
        /// <c>json</c> or <c>custom</c> are reserved for future releases.
        /// </summary>
        /// <remarks>
        /// <para><c>in-memory</c> handler provides in memory command and option descriptions.</para>
        /// <para><c>json</c> handler provides command and option descriptions in a JSON file.</para>
        /// <para>
        /// <c>custom</c> handler allows application authors to provide command and option descriptions from a custom
        /// store such as Entity framework or cloud databases REST API.
        /// </para>
        /// </remarks>
        public string StoreHandler { get; set; } = "in-memory";
    }
}