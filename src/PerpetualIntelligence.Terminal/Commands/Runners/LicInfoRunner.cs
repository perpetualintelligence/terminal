/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Licensing;
using PerpetualIntelligence.Shared.Licensing;
using PerpetualIntelligence.Shared.Extensions;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Runners
{
    /// <summary>
    /// The <c>lic</c> command displays the current licensing information.
    /// </summary>
    public class LicInfoRunner : CommandRunner<CommandRunnerResult>
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public LicInfoRunner(ILicenseExtractor licenseExractor, ILicenseChecker licenseChecker)
        {
            this.licenseExractor = licenseExractor;
            this.licenseChecker = licenseChecker;
        }

        /// <inheritdoc/>
        public override async Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            try
            {
                LicenseExtractorResult licResult = await licenseExractor.ExtractAsync(new LicenseExtractorContext());
                LicenseCheckerResult checkResult = await licenseChecker.CheckAsync(new LicenseCheckerContext(licResult.License));

                License license = licResult.License;

                {
                    // Print Details
                    TerminalHelper.WriteLineColor(ConsoleColor.Yellow, "License");
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "plan={0}", license.Plan);
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "usage={0}", license.Usage);
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "handler={0}", license.Handler);
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "provider_id={0}", license.ProviderId);
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "key_source={0}", license.LicenseKeySource);

                    if (license.LicenseKeySource == LicenseSources.JsonFile)
                    {
                        // Only display key file path
                        TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "key_file={0}", license.LicenseKey);
                    }
                }

                {
                    // Print Claims
                    TerminalHelper.WriteLineColor(ConsoleColor.Yellow, "Claims");
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "name={0}", license.Claims.Name);
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "tenant_id={0}", license.Claims.TenantId);
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "tenant_country={0}", license.Claims.TenantCountry);
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "object_id={0}", license.Claims.ObjectId ?? "-");
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "object_country={0}", license.Claims.ObjectCountry ?? "-");
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "authorized_party={0}", license.Claims.AuthorizedParty);
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "audience={0}", license.Claims.Audience);
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "issuer={0}", license.Claims.Issuer);
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "subject={0}", license.Claims.Subject);
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "jti={0}", license.Claims.Subject);
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "expiry={0}", license.Claims.Expiry.ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"));
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "issued_at={0}", license.Claims.IssuedAt.ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"));
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "not_before={0}", license.Claims.NotBefore.ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"));

                    if (license.Claims.Custom != null)
                    {
                        foreach (var kvp in license.Claims.Custom)
                        {
                            TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "{0}={1}", kvp.Key, kvp.Value.ToString());
                        }
                    }
                }

                {
                    // Print Limits
                    TerminalHelper.WriteLineColor(ConsoleColor.Yellow, "Limits");
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "terminal_limit={0}", PrintNumber(license.Limits.RootCommandLimit));
                    TerminalHelper.WriteLineColor(ConsoleColor.Red, "redistribution_limit={0}", PrintNumber(license.Limits.RedistributionLimit));
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "root_command_limit={0}", PrintNumber(license.Limits.RootCommandLimit));
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "grouped_command_limit={0}", PrintNumber(license.Limits.GroupedCommandLimit));
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "sub_command_limit={0}", PrintNumber(license.Limits.SubCommandLimit));
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "option_limit={0}", PrintNumber(license.Limits.OptionLimit));
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "option_alias={0}", license.Limits.OptionAlias.ToString());
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "default_option={0}", license.Limits.DefaultOption.ToString());
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "default_option_value={0}", license.Limits.DefaultOptionValue.ToString());
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "strict_data_type={0}", license.Limits.StrictDataType.ToString());
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "data_type_handlers={0}", license.Limits.DataTypeHandlers.JoinBySpace());
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "test_handlers={0}", license.Limits.TextHandlers.JoinBySpace());
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "error_handlers={0}", license.Limits.ErrorHandlers.JoinBySpace());
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "store_handlers={0}", license.Limits.StoreHandlers.JoinBySpace());
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "service_handlers={0}", license.Limits.ServiceHandlers.JoinBySpace());
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "license_handlers={0}", license.Limits.LicenseHandlers.JoinBySpace());

                    if (license.Claims.Custom != null)
                    {
                        foreach (var kvp in license.Claims.Custom)
                        {
                            TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "{0}={1}", kvp.Key, kvp.Value.ToString());
                        }
                    }
                }

                {
                    // Print Usage
                    TerminalHelper.WriteLineColor(ConsoleColor.Yellow, "Usage");
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "root_command={0}", checkResult.RootCommandCount);
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "grouped_command={0}", checkResult.CommandGroupCount);
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "sub_command={0}", checkResult.SubCommandCount);
                    TerminalHelper.WriteLineColor(ConsoleColor.Cyan, "option={0}", checkResult.OptionCount);
                }

                return new CommandRunnerResult();
            }
            finally
            {
                Console.ResetColor();
            }
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
        private readonly ILicenseExtractor licenseExractor;
    }
}