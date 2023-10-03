/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Licensing;
using System;
using System.Collections.Generic;

namespace PerpetualIntelligence.Terminal.Licensing
{
    /// <summary>
    /// Defines the licensing limits based on the <see cref="PiCliLicensePlans"/>.
    /// </summary>
    public sealed class LicenseLimits
    {
        /// <summary>
        /// The maximum options or options. Defaults to <c>null</c> or no limit.
        /// </summary>
        public long? OptionLimit { get; internal set; }

        /// <summary>
        /// Supports the command option data type checks. Defaults to <c>null</c> or no data type checks.
        /// </summary>
        public string[]? DataTypeHandlers { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public string[]? ErrorHandlers { get; internal set; }

        /// <summary>
        /// The maximum grouped commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public int? GroupedCommandLimit { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no redistributions.
        /// </summary>
        public string[]? LicenseHandlers { get; internal set; }

        /// <summary>
        /// The SaaS plan.
        /// </summary>
        public string? Plan { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no redistribution limit.
        /// </summary>
        public long? RedistributionLimit { get; internal set; }

        /// <summary>
        /// The maximum root commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public int? RootCommandLimit { get; internal set; }

        /// <summary>
        /// The service implantations.
        /// </summary>
        public string[]? ServiceHandlers { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no redistributions.
        /// </summary>
        public string[]? StoreHandlers { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public bool StrictDataType { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public long? SubCommandLimit { get; internal set; }

        /// <summary>
        /// The maximum terminals. Defaults to <c>null</c> or no limit.
        /// </summary>
        public int? TerminalLimit { get; internal set; }

        /// <summary>
        /// The maximum sub commands. Defaults to <c>null</c> or no limit.
        /// </summary>
        public string[]? TextHandlers { get; internal set; }

        /// <summary>
        /// Creates a new instance of <see cref="LicenseLimits"/> based on the specified SaaS plan.
        /// </summary>
        /// <param name="licensePlan">The license plan.</param>
        /// <param name="customClaims">The custom claims. Only used if SaaS plan is custom.</param>
        public static LicenseLimits Create(string licensePlan, IDictionary<string, object>? customClaims = null)
        {
            if (string.IsNullOrEmpty(licensePlan))
            {
                throw new System.ArgumentException($"'{nameof(licensePlan)}' cannot be null or empty.", nameof(licensePlan));
            }

            switch (licensePlan)
            {
                case PiCliLicensePlans.Demo:
                    {
                        return ForDemo();
                    }
                case PiCliLicensePlans.Micro:
                    {
                        return ForMicro();
                    }
                case PiCliLicensePlans.SMB:
                    {
                        return ForSMB();
                    }
                case PiCliLicensePlans.Enterprise:
                    {
                        return ForEnterprise();
                    }
                case PiCliLicensePlans.OnPremise:
                    {
                        return ForOnPremise();
                    }
                case PiCliLicensePlans.Unlimited:
                    {
                        return ForUnlimited();
                    }
                case PiCliLicensePlans.Custom:
                    {
                        if (customClaims == null)
                        {
                            throw new TerminalException(TerminalErrors.InvalidLicense, "The licensing for the custom SaaS plan requires a custom claims. saas_plan={0}", licensePlan);
                        }

                        return ForCustom(customClaims);
                    }
                default:
                    {
                        throw new TerminalException(TerminalErrors.InvalidLicense, "The licensing for the SaaS plan is not supported. saas_plan={0}", licensePlan);
                    }
            }
        }

        internal LicenseLimits()
        {
        }

        internal static LicenseLimits ForDemo()
        {
            return new()
            {
                Plan = PiCliLicensePlans.Demo,
                TerminalLimit = 1,
                RedistributionLimit = 0,
                RootCommandLimit = 1,
                GroupedCommandLimit = 5,
                SubCommandLimit = 25,
                OptionLimit = 500,

                StrictDataType = true,

                DataTypeHandlers = new[] { TerminalHandlers.DefaultHandler },
                TextHandlers = new[] { TerminalHandlers.UnicodeHandler, TerminalHandlers.AsciiHandler },
                ErrorHandlers = new[] { TerminalHandlers.DefaultHandler },
                StoreHandlers = new[] { TerminalHandlers.InMemoryHandler },
                ServiceHandlers = new[] { TerminalHandlers.DefaultHandler },
                LicenseHandlers = new[] { TerminalHandlers.OnlineLicenseHandler }
            };
        }

        internal static LicenseLimits ForCustom(IDictionary<string, object> customClaims)
        {
            LicenseLimits limits = new()
            {
                Plan = PiCliLicensePlans.Custom,

                TerminalLimit = Convert.ToInt16(customClaims["terminal_limit"]),
                RedistributionLimit = Convert.ToInt16(customClaims["redistribution_limit"]),
                RootCommandLimit = Convert.ToInt16(customClaims["root_command_limit"]),
                GroupedCommandLimit = Convert.ToInt16(customClaims["grouped_command_limit"]),
                SubCommandLimit = Convert.ToInt16(customClaims["sub_command_limit"]),
                OptionLimit = Convert.ToInt16(customClaims["option_limit"]),

                StrictDataType = Convert.ToBoolean(customClaims["strict_data_type"]),

                DataTypeHandlers = customClaims["data_type_handlers"].ToString().SplitBySpace(),
                TextHandlers = customClaims["text_handlers"].ToString().SplitBySpace(),
                ErrorHandlers = customClaims["error_handlers"].ToString().SplitBySpace(),
                StoreHandlers = customClaims["store_handlers"].ToString().SplitBySpace(),
                ServiceHandlers = customClaims["service_handlers"].ToString().SplitBySpace(),
                LicenseHandlers = customClaims["license_handlers"].ToString().SplitBySpace()
            };

            return limits;
        }

        internal static LicenseLimits ForEnterprise()
        {
            return new()
            {
                Plan = PiCliLicensePlans.Enterprise,
                TerminalLimit = 5,
                RedistributionLimit = 5000,
                RootCommandLimit = 3,
                GroupedCommandLimit = 20,
                SubCommandLimit = 100,
                OptionLimit = 2000,

                StrictDataType = true,

                DataTypeHandlers = new[] { TerminalHandlers.DefaultHandler, TerminalHandlers.CustomHandler },
                TextHandlers = new[] { TerminalHandlers.UnicodeHandler, TerminalHandlers.AsciiHandler },
                ErrorHandlers = new[] { TerminalHandlers.DefaultHandler, TerminalHandlers.CustomHandler },
                StoreHandlers = new[] { TerminalHandlers.InMemoryHandler, TerminalHandlers.JsonHandler, TerminalHandlers.CustomHandler },
                ServiceHandlers = new[] { TerminalHandlers.DefaultHandler, TerminalHandlers.CustomHandler },
                LicenseHandlers = new[] { TerminalHandlers.OnlineLicenseHandler, TerminalHandlers.OfflineLicenseHandler }
            };
        }

        internal static LicenseLimits ForOnPremise()
        {
            return new()
            {
                Plan = PiCliLicensePlans.OnPremise,
                TerminalLimit = 25,
                RedistributionLimit = 10000,
                RootCommandLimit = 5,
                GroupedCommandLimit = 50,
                SubCommandLimit = 250,
                OptionLimit = 5000,

                StrictDataType = true,

                DataTypeHandlers = new[] { TerminalHandlers.DefaultHandler, TerminalHandlers.CustomHandler },
                TextHandlers = new[] { TerminalHandlers.UnicodeHandler, TerminalHandlers.AsciiHandler },
                ErrorHandlers = new[] { TerminalHandlers.DefaultHandler, TerminalHandlers.CustomHandler },
                StoreHandlers = new[] { TerminalHandlers.InMemoryHandler, TerminalHandlers.JsonHandler, TerminalHandlers.CustomHandler },
                ServiceHandlers = new[] { TerminalHandlers.DefaultHandler, TerminalHandlers.CustomHandler },
                LicenseHandlers = new[] { TerminalHandlers.OnlineLicenseHandler, TerminalHandlers.OfflineLicenseHandler, TerminalHandlers.OnPremiseLicenseHandler }
            };
        }

        internal static LicenseLimits ForUnlimited()
        {
            return new()
            {
                Plan = PiCliLicensePlans.Unlimited,
                TerminalLimit = null,
                RedistributionLimit = null,
                RootCommandLimit = null,
                GroupedCommandLimit = null,
                SubCommandLimit = null,
                OptionLimit = null,

                StrictDataType = true,

                DataTypeHandlers = new[] { TerminalHandlers.DefaultHandler, TerminalHandlers.CustomHandler },
                TextHandlers = new[] { TerminalHandlers.UnicodeHandler, TerminalHandlers.AsciiHandler },
                ErrorHandlers = new[] { TerminalHandlers.DefaultHandler, TerminalHandlers.CustomHandler },
                StoreHandlers = new[] { TerminalHandlers.InMemoryHandler, TerminalHandlers.JsonHandler, TerminalHandlers.CustomHandler },
                ServiceHandlers = new[] { TerminalHandlers.DefaultHandler, TerminalHandlers.CustomHandler },
                LicenseHandlers = new[] { TerminalHandlers.OnlineLicenseHandler, TerminalHandlers.OfflineLicenseHandler, TerminalHandlers.OnPremiseLicenseHandler }
            };
        }

        internal static LicenseLimits ForMicro()
        {
            return new()
            {
                Plan = PiCliLicensePlans.Micro,
                TerminalLimit = 1,
                RedistributionLimit = 0,
                RootCommandLimit = 1,
                GroupedCommandLimit = 5,
                SubCommandLimit = 25,
                OptionLimit = 500,

                StrictDataType = false,

                DataTypeHandlers = null,
                TextHandlers = new[] { TerminalHandlers.UnicodeHandler, TerminalHandlers.AsciiHandler },
                ErrorHandlers = new[] { TerminalHandlers.DefaultHandler },
                StoreHandlers = new[] { TerminalHandlers.InMemoryHandler },
                ServiceHandlers = new[] { TerminalHandlers.DefaultHandler },
                LicenseHandlers = new[] { TerminalHandlers.OnlineLicenseHandler }
            };
        }

        internal static LicenseLimits ForSMB()
        {
            return new()
            {
                Plan = PiCliLicensePlans.SMB,
                TerminalLimit = 3,
                RedistributionLimit = 1000,
                RootCommandLimit = 1,
                GroupedCommandLimit = 10,
                SubCommandLimit = 50,
                OptionLimit = 1000,

                StrictDataType = true,

                DataTypeHandlers = new[] { TerminalHandlers.DefaultHandler },
                TextHandlers = new[] { TerminalHandlers.UnicodeHandler, TerminalHandlers.AsciiHandler },
                ErrorHandlers = new[] { TerminalHandlers.DefaultHandler },
                StoreHandlers = new[] { TerminalHandlers.InMemoryHandler, TerminalHandlers.JsonHandler },
                ServiceHandlers = new[] { TerminalHandlers.DefaultHandler },
                LicenseHandlers = new[] { TerminalHandlers.OnlineLicenseHandler }
            };
        }
    }
}