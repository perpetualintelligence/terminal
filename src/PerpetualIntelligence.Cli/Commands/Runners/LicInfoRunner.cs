/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Cli.Services;
using PerpetualIntelligence.Protocols.Licensing;
using PerpetualIntelligence.Shared.Extensions;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Runners
{
    /// <summary>
    /// The <c>lic</c> command displays the current licensing information.
    /// </summary>
    public class LicInfoRunner : ICommandRunner
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
        public async Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
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
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "check_mode={0}", license.CheckMode);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "provider_id={0}", license.ProviderId);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "key_source={0}", license.LicenseKeySource);

                    if (license.LicenseKeySource == SaaSKeySources.JsonFile)
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
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "expiry={0}", license.Claims.Expiry.GetValueOrDefault().ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"));
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "issued_at={0}", license.Claims.IssuedAt.GetValueOrDefault().ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"));
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "not_before={0}", license.Claims.NotBefore.GetValueOrDefault().ToLocalTime().ToString("dd-MMM-yyyy HH:mm:ss"));

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
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "root_command_limit={0}", PrintNumber(license.Limits.RootCommandLimit));
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "command_group_limit={0}", PrintNumber(license.Limits.CommandGroupLimit));
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "sub_command_limit={0}", PrintNumber(license.Limits.SubCommandLimit));
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "argument_limit={0}", PrintNumber(license.Limits.ArgumentLimit));
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "data_type_checks={0}", license.Limits.DataTypeChecks.JoinBySpace());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "strict_data_type={0}", license.Limits.StrictDataType.ToString());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "default_argument={0}", license.Limits.DefaultArgument.ToString());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "default_argument_value={0}", license.Limits.DefaultArgumentValue.ToString());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "unicode_support={0}", license.Limits.UnicodeSupport.JoinBySpace());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "error_handling={0}", license.Limits.ErrorHandling.JoinBySpace());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "stores={0}", license.Limits.Stores.JoinBySpace());
                    ConsoleHelper.WriteLineColor(ConsoleColor.Red, "redistribution_limit={0}", PrintNumber(license.Limits.RedistributionLimit));
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "service_implementations={0}", license.Limits.ServiceImplementations.JoinBySpace());

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
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "command_group={0}", checkResult.CommandGroupCount);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "sub_command={0}", checkResult.SubCommandCount);
                    ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, "argument={0}", checkResult.ArgumentCount);
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
