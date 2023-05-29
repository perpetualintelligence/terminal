/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Licensing;
using PerpetualIntelligence.Terminal.Services;
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
                    ConsoleHelper.WriteLineColor(ConsoleColor.Yellow, "License");
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "plan={0}", license.Plan);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "usage={0}", license.Usage);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "handler={0}", license.Handler);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "provider_id={0}", license.ProviderId);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "key_source={0}", license.LicenseKeySource);

                    if (license.LicenseKeySource == LicenseSources.JsonFile)
                    {
                        // Only display key file path
                        ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "key_file={0}", license.LicenseKey);
                    }
                }

                {
                    // Print Claims
                    ConsoleHelper.WriteLineColor(ConsoleColor.Yellow, "Claims");
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "name={0}", license.Claims.Name);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "tenant_id={0}", license.Claims.TenantId);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "tenant_country={0}", license.Claims.TenantCountry);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "object_id={0}", license.Claims.ObjectId ?? "-");
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "object_country={0}", license.Claims.ObjectCountry ?? "-");
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "authorized_party={0}", license.Claims.AuthorizedParty);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "audience={0}", license.Claims.Audience);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "issuer={0}", license.Claims.Issuer);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "subject={0}", license.Claims.Subject);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "jti={0}", license.Claims.Subject);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "expiry={0}", license.Claims.Expiry.ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"));
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "issued_at={0}", license.Claims.IssuedAt.ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"));
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "not_before={0}", license.Claims.NotBefore.ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"));

                    if (license.Claims.Custom != null)
                    {
                        foreach (var kvp in license.Claims.Custom)
                        {
                            ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "{0}={1}", kvp.Key, kvp.Value.ToString());
                        }
                    }
                }

                {
                    // Print Limits
                    ConsoleHelper.WriteLineColor(ConsoleColor.Yellow, "Limits");
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "terminal_limit={0}", PrintNumber(license.Limits.RootCommandLimit));
                    ConsoleHelper.WriteLineColor(ConsoleColor.Red, "redistribution_limit={0}", PrintNumber(license.Limits.RedistributionLimit));
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "root_command_limit={0}", PrintNumber(license.Limits.RootCommandLimit));
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "grouped_command_limit={0}", PrintNumber(license.Limits.GroupedCommandLimit));
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "sub_command_limit={0}", PrintNumber(license.Limits.SubCommandLimit));
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "option_limit={0}", PrintNumber(license.Limits.OptionLimit));
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "option_alias={0}", license.Limits.OptionAlias.ToString());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "default_option={0}", license.Limits.DefaultOption.ToString());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "default_option_value={0}", license.Limits.DefaultOptionValue.ToString());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "strict_data_type={0}", license.Limits.StrictDataType.ToString());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "data_type_handlers={0}", license.Limits.DataTypeHandlers.JoinBySpace());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "test_handlers={0}", license.Limits.TextHandlers.JoinBySpace());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "error_handlers={0}", license.Limits.ErrorHandlers.JoinBySpace());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "store_handlers={0}", license.Limits.StoreHandlers.JoinBySpace());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "service_handlers={0}", license.Limits.ServiceHandlers.JoinBySpace());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "license_handlers={0}", license.Limits.LicenseHandlers.JoinBySpace());

                    if (license.Claims.Custom != null)
                    {
                        foreach (var kvp in license.Claims.Custom)
                        {
                            ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "{0}={1}", kvp.Key, kvp.Value.ToString());
                        }
                    }
                }

                {
                    // Print Usage
                    ConsoleHelper.WriteLineColor(ConsoleColor.Yellow, "Usage");
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "root_command={0}", checkResult.RootCommandCount);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "grouped_command={0}", checkResult.CommandGroupCount);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "sub_command={0}", checkResult.SubCommandCount);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "option={0}", checkResult.OptionCount);
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