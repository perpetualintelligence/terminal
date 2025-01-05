/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using OneImlx.Shared.Licensing;

namespace OneImlx.Terminal.Licensing
{
    /// <summary>
    /// Defines the licensing quota based on the <see cref="TerminalLicensePlans"/>.
    /// </summary>
    public sealed class LicenseQuota
    {
        /// <summary>
        /// The maximum commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public int? CommandLimit { get; internal set; }

        /// <summary>
        /// The terminal driver mode. Defaults to <c>false</c> or not supported.
        /// </summary>
        public bool Driver { get; internal set; }

        /// <summary>
        /// The licensed features.
        /// </summary>
        public Dictionary<string, string[]> Features { get; internal set; } = [];

        /// <summary>
        /// The maximum arguments and options combined. Defaults to <c>null</c> for no limit.
        /// </summary>
        public int? InputLimit { get; internal set; }

        /// <summary>
        /// The terminal integration mode. Defaults to <c>false</c> or not supported.
        /// </summary>
        public bool Integration { get; internal set; }

        /// <summary>
        /// The maximum quota. Defaults to <c>null</c> or no limit.
        /// </summary>
        public Dictionary<string, long?> Limits { get; internal set; } = [];

        /// <summary>
        /// The license plan.
        /// </summary>
        public string? Plan { get; internal set; }

        /// <summary>
        /// The maximum redistributions. Defaults to <c>null</c> or no redistribution limit.
        /// </summary>
        public long? RedistributionLimit { get; internal set; }

        /// <summary>
        /// The string date type. Defaults to <c>null</c> or no limit.
        /// </summary>
        public bool StrictDataType { get; internal set; }

        /// <summary>
        /// The licensed switches.
        /// </summary>
        public Dictionary<string, bool> Switches { get; internal set; } = [];

        /// <summary>
        /// The maximum terminals. Defaults to <c>null</c> or no limit.
        /// </summary>
        public int? TerminalLimit { get; internal set; }

        /// <summary>
        /// Creates a new instance of <see cref="LicenseQuota"/> based on the specified SaaS plan.
        /// </summary>
        /// <param name="licensePlan">The license plan.</param>
        /// <param name="customClaims">The custom claims. Only used if SaaS plan is custom.</param>
        public static LicenseQuota Create(string licensePlan, IDictionary<string, object>? customClaims = null)
        {
            switch (licensePlan)
            {
                case TerminalLicensePlans.Demo:
                    {
                        return ForDemo();
                    }
                case TerminalLicensePlans.Solo:
                    {
                        return ForSolo();
                    }
                case TerminalLicensePlans.Micro:
                    {
                        return ForMicro();
                    }
                case TerminalLicensePlans.Smb:
                    {
                        return ForSmb();
                    }
                case TerminalLicensePlans.Enterprise:
                    {
                        return ForEnterprise();
                    }
                case TerminalLicensePlans.Corporate:
                    {
                        return ForCorporate();
                    }
                case TerminalLicensePlans.Custom:
                    {
                        if (customClaims == null)
                        {
                            throw new TerminalException(TerminalErrors.InvalidLicense, "The licensing for the custom plan requires a custom claims. plan={0}", licensePlan);
                        }

                        return ForCustom(customClaims);
                    }
                default:
                    {
                        throw new TerminalException(TerminalErrors.InvalidLicense, "The license for the plan is not supported. plan={0}", licensePlan);
                    }
            }
        }

        private static LicenseQuota ForCorporate()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Corporate,

                Limits = new Dictionary<string, long?>
                {
                    { "terminal", 15 },
                    { "command", null },
                    { "input", null },
                    { "redistribution", null }
                },

                Switches = new Dictionary<string, bool>
                {
                    { "datatype", true },
                    { "driver", true },
                    { "integration", true }
                },

                Features = new Dictionary<string, string[]>
                {
                    { "authentication", new[] { "msal", "oauth", "oidc" } },
                    { "encoding", new [] { "ascii", "utf8", "utf16-le", "utf16-be", "utf32" } },
                    { "store", new [] { "memory", "custom" } },
                    { "router", new [] { "console", "tcp", "udp", "grpc", "http", "custom" } },
                    { "deployment", new [] { "standard", "isolated" } },
                }
            };
        }

        private static LicenseQuota ForCustom(IDictionary<string, object> customClaims)
        {
            LicenseQuota quota = new()
            {
                Plan = TerminalLicensePlans.Custom,

                TerminalLimit = Convert.ToInt32(customClaims["terminal_limit"]),
                CommandLimit = Convert.ToInt32(customClaims["command_limit"]),
                InputLimit = Convert.ToInt32(customClaims["input_limit"]),
                RedistributionLimit = Convert.ToInt32(customClaims["redistribution_limit"]),

                StrictDataType = Convert.ToBoolean(customClaims["strict_data_type"]),
                Driver = Convert.ToBoolean(customClaims["driver"]),
                Integration = Convert.ToBoolean(customClaims["integration"]),
            };

            return quota;
        }

        private static LicenseQuota ForDemo()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Demo,

                TerminalLimit = 1,
                CommandLimit = 25,
                InputLimit = 250,
                RedistributionLimit = 0,

                StrictDataType = true,
                Driver = true,
                Integration = true,
            };
        }

        private static LicenseQuota ForEnterprise()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Enterprise,

                TerminalLimit = 10,
                CommandLimit = 300,
                InputLimit = 6000,
                RedistributionLimit = 15000,

                StrictDataType = true,
                Driver = true,
                Integration = true,
            };
        }

        private static LicenseQuota ForMicro()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Micro,

                TerminalLimit = 3,
                CommandLimit = 50,
                InputLimit = 500,
                RedistributionLimit = 1000,

                StrictDataType = true,
                Driver = false,
                Integration = false
            };
        }

        private static LicenseQuota ForSmb()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Smb,

                TerminalLimit = 5,
                CommandLimit = 100,
                InputLimit = 2000,
                RedistributionLimit = 5000,

                StrictDataType = true,
                Driver = true,
                Integration = false
            };
        }

        private static LicenseQuota ForSolo()
        {
            return new()
            {
                Plan = TerminalLicensePlans.Solo,

                TerminalLimit = 1,
                CommandLimit = 25,
                InputLimit = 250,
                RedistributionLimit = 0,

                StrictDataType = false,
                Driver = false,
                Integration = false
            };
        }
    }
}
