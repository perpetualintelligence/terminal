/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Shared.Licensing;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using System;
using System.Collections.Generic;

namespace PerpetualIntelligence.Cli.Licensing
{
    /// <summary>
    /// Defines the licensing limits based on the <see cref="LicensePlans"/>.
    /// </summary>
    public sealed class LicenseLimits
    {
        /// <summary>
        /// Checks <see cref="ExtractorOptions.ArgumentAlias"/> option.
        /// </summary>
        public bool ArgumentAlias { get; internal set; }

        /// <summary>
        /// The maximum options or options. Defaults to <c>null</c> or no limit.
        /// </summary>
        public long? ArgumentLimit { get; internal set; }

        /// <summary>
        /// Supports the command option data type checks. Defaults to <c>null</c> or no data type checks.
        /// </summary>
        public string[]? DataTypeHandlers { get; internal set; }

        /// <summary>
        /// Supports the default command option. Defaults to <c>false</c> or no default options.
        /// </summary>
        public bool DefaultArgument { get; internal set; }

        /// <summary>
        /// Supports the default option value. Defaults to <c>false</c> or no default option value.
        /// </summary>
        public bool DefaultArgumentValue { get; internal set; }

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
        /// <param name="saasPlan">The SaaS plan.</param>
        /// <param name="customClaims">The custom claims. Only used if SaaS plan is custom.</param>
        public static LicenseLimits Create(string saasPlan, IDictionary<string, object>? customClaims = null)
        {
            if (string.IsNullOrEmpty(saasPlan))
            {
                throw new System.ArgumentException($"'{nameof(saasPlan)}' cannot be null or empty.", nameof(saasPlan));
            }

            switch (saasPlan)
            {
                case LicensePlans.Community:
                    {
                        return ForCommunity();
                    }
                case LicensePlans.Micro:
                    {
                        return ForMicro();
                    }
                case LicensePlans.SMB:
                    {
                        return ForSMB();
                    }
                case LicensePlans.Enterprise:
                    {
                        return ForEnterprise();
                    }
                case LicensePlans.ISV:
                    {
                        return ForISV();
                    }
                case LicensePlans.ISVU:
                    {
                        return ForISVU();
                    }
                case LicensePlans.Custom:
                    {
                        if (customClaims == null)
                        {
                            throw new ErrorException(Errors.InvalidLicense, "The licensing for the custom SaaS plan requires a custom claims. saas_plan={0}", saasPlan);
                        }

                        return ForCustom(customClaims);
                    }
                default:
                    {
                        throw new ErrorException(Errors.InvalidLicense, "The licensing for the SaaS plan is not supported. saas_plan={0}", saasPlan);
                    }
            }
        }

        /// <summary>
        /// Gets the demo claims.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> DemoClaims()
        {
            Dictionary<string, object> claims = new()
            {
                { "terminal_limit", 1 },
                { "redistribution_limit", 0 },
                { "root_command_limit", 1 },
                { "grouped_command_limit", 2 },
                { "sub_command_limit", 15 },
                { "argument_limit", 100 },

                { "argument_alias", true },
                { "default_argument", true },
                { "default_argument_value", true },
                { "strict_data_type", true },

                { "data_type_handlers", "default" },
                { "text_handlers", new[] { "unicode", "ascii" }.JoinBySpace() },
                { "error_handlers", "default" },
                { "store_handlers", "in-memory" },
                { "service_handlers", "default" },
                { "license_handlers", "online" },

                { "currency", "USD" },
                { "monthly_price", 0.0 },
                { "yearly_price", 0.0 }
            };

            return claims;
        }

        internal LicenseLimits()
        {
        }

        internal static LicenseLimits ForCommunity()
        {
            return new()
            {
                Plan = LicensePlans.Community,
                TerminalLimit = 1,
                RedistributionLimit = 0,
                RootCommandLimit = 1,
                GroupedCommandLimit = 5,
                SubCommandLimit = 25,
                ArgumentLimit = 500,

                ArgumentAlias = false,
                DefaultArgument = false,
                DefaultArgumentValue = false,
                StrictDataType = false,

                DataTypeHandlers = null,
                TextHandlers = new[] { "unicode", "ascii" },
                ErrorHandlers = new[] { "default" },
                StoreHandlers = new[] { "in-memory" },
                ServiceHandlers = new[] { "default" },
                LicenseHandlers = new[] { "online" }
            };
        }

        internal static LicenseLimits ForCustom(IDictionary<string, object> customClaims)
        {
            LicenseLimits limits = new()
            {
                Plan = LicensePlans.Custom,

                TerminalLimit = Convert.ToInt16(customClaims["terminal_limit"]),
                RedistributionLimit = Convert.ToInt16(customClaims["redistribution_limit"]),
                RootCommandLimit = Convert.ToInt16(customClaims["root_command_limit"]),
                GroupedCommandLimit = Convert.ToInt16(customClaims["grouped_command_limit"]),
                SubCommandLimit = Convert.ToInt16(customClaims["sub_command_limit"]),
                ArgumentLimit = Convert.ToInt16(customClaims["argument_limit"]),

                ArgumentAlias = Convert.ToBoolean(customClaims["argument_alias"]),
                DefaultArgument = Convert.ToBoolean(customClaims["default_argument"]),
                DefaultArgumentValue = Convert.ToBoolean(customClaims["default_argument_value"]),
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
                Plan = LicensePlans.Enterprise,
                TerminalLimit = 5,
                RedistributionLimit = 5000,
                RootCommandLimit = 3,
                GroupedCommandLimit = 20,
                SubCommandLimit = 100,
                ArgumentLimit = 2000,

                ArgumentAlias = true,
                DefaultArgument = true,
                DefaultArgumentValue = true,
                StrictDataType = true,

                DataTypeHandlers = new[] { "default", "custom" },
                TextHandlers = new[] { "unicode", "ascii" },
                ErrorHandlers = new[] { "default", "custom" },
                StoreHandlers = new[] { "in-memory", "json", "custom" },
                ServiceHandlers = new[] { "default", "custom" },
                LicenseHandlers = new[] { "online", "offline" }
            };
        }

        internal static LicenseLimits ForISV()
        {
            return new()
            {
                Plan = LicensePlans.ISV,
                TerminalLimit = 25,
                RedistributionLimit = 10000,
                RootCommandLimit = 5,
                GroupedCommandLimit = 50,
                SubCommandLimit = 250,
                ArgumentLimit = 5000,

                ArgumentAlias = true,
                DefaultArgument = true,
                DefaultArgumentValue = true,
                StrictDataType = true,

                DataTypeHandlers = new[] { "default", "custom" },
                TextHandlers = new[] { "unicode", "ascii" },
                ErrorHandlers = new[] { "default", "custom" },
                StoreHandlers = new[] { "in-memory", "json", "custom" },
                ServiceHandlers = new[] { "default", "custom" },
                LicenseHandlers = new[] { "online", "offline", "byol" }
            };
        }

        internal static LicenseLimits ForISVU()
        {
            return new()
            {
                Plan = LicensePlans.ISVU,
                TerminalLimit = null,
                RedistributionLimit = null,
                RootCommandLimit = null,
                GroupedCommandLimit = null,
                SubCommandLimit = null,
                ArgumentLimit = null,

                ArgumentAlias = true,
                DefaultArgument = true,
                DefaultArgumentValue = true,
                StrictDataType = true,

                DataTypeHandlers = new[] { "default", "custom" },
                TextHandlers = new[] { "unicode", "ascii" },
                ErrorHandlers = new[] { "default", "custom" },
                StoreHandlers = new[] { "in-memory", "json", "custom" },
                ServiceHandlers = new[] { "default", "custom" },
                LicenseHandlers = new[] { "online", "offline", "byol" }
            };
        }

        internal static LicenseLimits ForMicro()
        {
            return new()
            {
                Plan = LicensePlans.Micro,
                TerminalLimit = 1,
                RedistributionLimit = 0,
                RootCommandLimit = 1,
                GroupedCommandLimit = 5,
                SubCommandLimit = 25,
                ArgumentLimit = 500,

                ArgumentAlias = false,
                DefaultArgument = false,
                DefaultArgumentValue = false,
                StrictDataType = false,

                DataTypeHandlers = null,
                TextHandlers = new[] { "unicode", "ascii" },
                ErrorHandlers = new[] { "default" },
                StoreHandlers = new[] { "in-memory" },
                ServiceHandlers = new[] { "default" },
                LicenseHandlers = new[] { "online" }
            };
        }

        internal static LicenseLimits ForSMB()
        {
            return new()
            {
                Plan = LicensePlans.SMB,
                TerminalLimit = 3,
                RedistributionLimit = 1000,
                RootCommandLimit = 1,
                GroupedCommandLimit = 10,
                SubCommandLimit = 50,
                ArgumentLimit = 1000,

                ArgumentAlias = true,
                DefaultArgument = true,
                DefaultArgumentValue = true,
                StrictDataType = true,

                DataTypeHandlers = new[] { "default" },
                TextHandlers = new[] { "unicode", "ascii" },
                ErrorHandlers = new[] { "default" },
                StoreHandlers = new[] { "in-memory", "json" },
                ServiceHandlers = new[] { "default" },
                LicenseHandlers = new[] { "online" }
            };
        }
    }
}