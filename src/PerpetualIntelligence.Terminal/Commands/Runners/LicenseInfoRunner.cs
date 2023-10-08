/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Licensing;
using PerpetualIntelligence.Terminal.Licensing;
using PerpetualIntelligence.Terminal.Runtime;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Runners
{
    /// <summary>
    /// The <c>lic</c> command displays the current licensing information.
    /// </summary>
    public class LicenseInfoRunner : CommandRunner<CommandRunnerResult>
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public LicenseInfoRunner(ITerminalConsole terminalConsole, ILicenseExtractor licenseExractor, ILicenseChecker licenseChecker)
        {
            this.terminalConsole = terminalConsole;
            this.licenseExractor = licenseExractor;
            this.licenseChecker = licenseChecker;
        }

        /// <inheritdoc/>
        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            LicenseExtractorResult licResult = await licenseExractor.ExtractAsync(new LicenseExtractorContext());
            LicenseCheckerResult checkResult = await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(licResult.License));

            License license = licResult.License;

            {
                // Print Details
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "License");
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "plan={0}", license.Plan);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "usage={0}", license.Usage);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "handler={0}", license.Handler);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "provider_id={0}", license.ProviderId);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "key_source={0}", license.LicenseKeySource);

                if (license.LicenseKeySource == LicenseSources.JsonFile)
                {
                    // Only display key file path
                    await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "key_file={0}", license.LicenseKey);
                }
            }

            {
                // Print Claims
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "Claims");
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "name={0}", license.Claims.Name);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "tenant_id={0}", license.Claims.TenantId);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "tenant_country={0}", license.Claims.TenantCountry);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "object_id={0}", license.Claims.ObjectId ?? "-");
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "object_country={0}", license.Claims.ObjectCountry ?? "-");
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "authorized_party={0}", license.Claims.AuthorizedParty);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "audience={0}", license.Claims.Audience);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "issuer={0}", license.Claims.Issuer);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "subject={0}", license.Claims.Subject);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "jti={0}", license.Claims.Subject);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "expiry={0}", license.Claims.Expiry.ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"));
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "issued_at={0}", license.Claims.IssuedAt.ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"));
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "not_before={0}", license.Claims.NotBefore.ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"));

                if (license.Claims.Custom != null)
                {
                    foreach (var kvp in license.Claims.Custom)
                    {
                        await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "{0}={1}", kvp.Key, kvp.Value.ToString());
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
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "data_type_handlers={0}", license.Limits.DataTypeHandlers.JoinBySpace());
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "test_handlers={0}", license.Limits.TextHandlers.JoinBySpace());
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "error_handlers={0}", license.Limits.ErrorHandlers.JoinBySpace());
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "store_handlers={0}", license.Limits.StoreHandlers.JoinBySpace());
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "service_handlers={0}", license.Limits.ServiceHandlers.JoinBySpace());
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "license_handlers={0}", license.Limits.LicenseHandlers.JoinBySpace());

                if (license.Claims.Custom != null)
                {
                    foreach (var kvp in license.Claims.Custom)
                    {
                        await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "{0}={1}", kvp.Key, kvp.Value.ToString());
                    }
                }
            }

            {
                // Print Usage
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "Usage");
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "root_command={0}", checkResult.RootCommandCount);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "grouped_command={0}", checkResult.CommandGroupCount);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "sub_command={0}", checkResult.SubCommandCount);
                await terminalConsole.WriteLineColorAsync(ConsoleColor.Cyan, "option={0}", checkResult.OptionCount);
            }

            return new CommandRunnerResult();
        }

        private string PrintNumber(object? nullableNumber)
        {
            if (nullableNumber == null)
            {
                return "INFINITE";
            }
            else
            {
                return nullableNumber.ToString();
            }
        }

        private readonly ILicenseChecker licenseChecker;
        private readonly ITerminalConsole terminalConsole;
        private readonly ILicenseExtractor licenseExractor;
    }
}