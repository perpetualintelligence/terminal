/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Commands.Runners
{
    /// <summary>
    /// The default license info runner that outputs the current licensing information to the <see cref="ITerminalConsole"/>.
    /// </summary>
    public class LicenseInfoRunner : CommandRunner<CommandRunnerResult>
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public LicenseInfoRunner(ITerminalConsole terminalConsole, ILicenseChecker licenseChecker)
        {
            this.terminalConsole = terminalConsole;
            this.licenseChecker = licenseChecker;
        }

        /// <inheritdoc/>
        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRouterContext context)
        {
            // Recheck the license to get the current consumption.
            // TODO: This should be tolerant of over-consumption since it is printing the usage. At present CheckLicenseAsync
            // throw exception on over-consumption.
            License license = context.EnsureLicense();
            LicenseCheckerResult checkResult = await licenseChecker.CheckLicenseAsync(license);

            {
                // Print Details
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "License");
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "plan={0}", license.Plan);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "usage={0}", license.Usage);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "mode={0}", license.Claims.Mode);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "deployment={0}", license.Claims.Deployment ?? "");
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "file={0}", license.LicenseKey);
            }

            {
                // Print Claims
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "Claims");
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "name={0}", license.Claims.TenantName);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "tenant_id={0}", license.Claims.TenantId);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "tenant_country={0}", license.Claims.TenantCountry);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "authorized_party={0}", license.Claims.AuthorizedParty);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "audience={0}", license.Claims.Audience);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "issuer={0}", license.Claims.Issuer);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "subject={0}", license.Claims.Subject);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "jti={0}", license.Claims.Subject);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "iat={0}", license.Claims.IssuedAt.ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"));
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "exp={0}", license.Claims.ExpiryAt.ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"));
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "nbf={0}", license.Claims.NotBefore.ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"));

                if (license.Claims.Custom != null)
                {
                    foreach (var kvp in license.Claims.Custom)
                    {
                        await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "{0}={1}", kvp.Key, kvp.Value.ToString() ?? "<null>");
                    }
                }
            }

            {
                // Print Limits
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "Limits");
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "terminal_limit={0}", PrintNumber(license.Limits.RootCommandLimit));
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Red, "redistribution_limit={0}", PrintNumber(license.Limits.RedistributionLimit));
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "root_command_limit={0}", PrintNumber(license.Limits.RootCommandLimit));
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "grouped_command_limit={0}", PrintNumber(license.Limits.GroupedCommandLimit));
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "sub_command_limit={0}", PrintNumber(license.Limits.SubCommandLimit));
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "option_limit={0}", PrintNumber(license.Limits.OptionLimit));
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "strict_data_type={0}", license.Limits.StrictDataType.ToString());
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "authentication={0}", license.Limits.Authentication.ToString() ?? "<null>");

                if (license.Claims.Custom != null)
                {
                    foreach (var kvp in license.Claims.Custom)
                    {
                        await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "{0}={1}", kvp.Key, kvp.Value.ToString() ?? "<null>");
                    }
                }
            }

            {
                // Print Usage
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "Usage");
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "root_command={0}", checkResult.RootCommandCount);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "command_groups={0}", checkResult.CommandGroupCount);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "sub_command={0}", checkResult.SubCommandCount);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "option={0}", checkResult.OptionCount);
            }

            return new CommandRunnerResult();
        }

        private string PrintNumber(object? nullableNumber)
        {
            if (nullableNumber == null)
            {
                return "Unlimited";
            }
            else
            {
                return nullableNumber.ToString()!;
            }
        }

        private readonly ILicenseChecker licenseChecker;
        private readonly ITerminalConsole terminalConsole;
    }
}
