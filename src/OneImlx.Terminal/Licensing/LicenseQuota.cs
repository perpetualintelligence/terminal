/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Licensing
{
    /// <summary>
    /// Defines the licensing quota based on the <see cref="ProductCatalog.TerminalFramework"/> license plans.
    /// </summary>
    public sealed class LicenseQuota
    {
        /// <summary>
        /// The authentication methods.
        /// </summary>
        public IReadOnlyList<string> Authentications => Features["authentications"];

        /// <summary>
        /// The maximum commands. Returns <c>null</c> for no limit.
        /// </summary>
        public int? CommandLimit => (int?)Limits["commands"];

        /// <summary>
        /// The strict date type. Returns <c>false</c> if not supported.
        /// </summary>
        public bool DataType => Switches["datatype"];

        /// <summary>
        /// The deployment methods.
        /// </summary>
        public IReadOnlyList<string> Deployments => Features["deployments"];

        /// <summary>
        /// The terminal driver mode. Returns <c>false</c> if not supported.
        /// </summary>
        public bool Driver => Switches["driver"];

        /// <summary>
        /// The terminal dynamics command mode. Defaults to <c>false</c> or not supported.
        /// </summary>
        public bool Dynamics => Switches["dynamics"];

        /// <summary>
        /// The text encoding.
        /// </summary>
        public IReadOnlyList<string> Encodings => Features["encodings"];

        /// <summary>
        /// The licensed features.
        /// </summary>
        public IReadOnlyDictionary<string, string[]> Features { get; internal set; } = new Dictionary<string, string[]>();

        /// <summary>
        /// The maximum arguments and options combined.Returns <c>null</c> for no limit.
        /// </summary>
        public int? InputLimit
        {
            get
            {
                return Limits["inputs"] == null ? null : Convert.ToInt32(Limits["inputs"]);
            }
        }

        /// <summary>
        /// The maximum quota. Defaults to <c>null</c> or no limit.
        /// </summary>
        public IReadOnlyDictionary<string, object?> Limits { get; internal set; } = new Dictionary<string, object?>();

        /// <summary>
        /// The license plan.
        /// </summary>
        public string? Plan { get; internal set; }

        /// <summary>
        /// The maximum redistributions. Defaults to <c>null</c> or no redistribution limit.
        /// </summary>
        public long? RedistributionLimit
        {
            get
            {
                return Limits["redistributions"] == null ? null : Convert.ToInt64(Limits["redistributions"]);
            }
        }

        /// <summary>
        /// The terminal routing methods.
        /// </summary>
        public IReadOnlyList<string> Routers => Features["routers"];

        /// <summary>
        /// The terminal command stores.
        /// </summary>
        public IReadOnlyList<string> Stores => Features["stores"];

        /// <summary>
        /// The licensed switches.
        /// </summary>
        public IReadOnlyDictionary<string, bool> Switches { get; internal set; } = new Dictionary<string, bool>();

        /// <summary>
        /// The maximum terminals. Defaults to <c>null</c> or no limit.
        /// </summary>
        public int? TerminalLimit
        {
            get
            {
                return Limits["terminals"] == null ? null : Convert.ToInt32(Limits["terminals"]);
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="LicenseQuota"/> based on the specified SaaS plan.
        /// </summary>
        /// <param name="licensePlan">The license plan.</param>
        /// <param name="customClaims">The custom claims. Only used if SaaS plan is custom.</param>
        public static LicenseQuota Create(string licensePlan, IDictionary<string, object>? customClaims = null)
        {
            // Custom claims are required for the custom plan.
            if (customClaims == null && licensePlan == ProductCatalog.TerminalPlanCustom)
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The licensing for the custom plan requires a custom claims. plan={0}", licensePlan);
            }

            if (customClaims != null && licensePlan != ProductCatalog.TerminalPlanCustom)
            {
                throw new TerminalException(TerminalErrors.InvalidLicense, "The custom claims are valid only for custom plan. plan={0}", licensePlan);
            }

            switch (licensePlan)
            {
                case ProductCatalog.TerminalPlanDemo:
                    {
                        return ForDemo();
                    }
                case ProductCatalog.TerminalPlanSolo:
                    {
                        return ForSolo();
                    }
                case ProductCatalog.TerminalPlanMicro:
                    {
                        return ForMicro();
                    }
                case ProductCatalog.TerminalPlanSmb:
                    {
                        return ForSmb();
                    }
                case ProductCatalog.TerminalPlanEnterprise:
                    {
                        return ForEnterprise();
                    }
                case ProductCatalog.TerminalPlanCorporate:
                    {
                        return ForCorporate();
                    }
                case ProductCatalog.TerminalPlanCustom:
                    {
                        return ForCustom(customClaims!);
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
                Plan = ProductCatalog.TerminalPlanCorporate,

                Limits = new Dictionary<string, object?>
                {
                    { "terminals", 15 },
                    { "commands", null },
                    { "inputs", null },
                    { "redistributions", null }
                },

                Switches = new Dictionary<string, bool>
                {
                    { "datatype", true },
                    { "driver", true },
                    { "dynamics", true }
                },

                Features = new Dictionary<string, string[]>
                {
                    { "authentications", new[] { "msal", "oauth", "oidc", "none" } },
                    { "encodings", new [] { "ascii", "utf8", "utf16", "utf32" } },
                    { "stores", new [] { "memory", "custom" } },
                    { "routers", new [] { "console", "tcp", "udp", "grpc", "http", "custom" } },
                    { "deployments", new [] { "standard", "air_gapped" } },
                }
            };
        }

        private static LicenseQuota ForCustom(IDictionary<string, object> customClaims)
        {
            return new()
            {
                Plan = ProductCatalog.TerminalPlanCustom,

                Limits = new Dictionary<string, object?>
                {
                    { "terminals", Convert.ToInt32(customClaims["terminals"]) },
                    { "commands", Convert.ToInt32(customClaims["commands"]) },
                    { "inputs", Convert.ToInt32(customClaims["inputs"]) },
                    { "redistributions", Convert.ToInt64(customClaims["redistributions"]) }
                },

                Switches = new Dictionary<string, bool>
                {
                    { "datatype", Convert.ToBoolean(customClaims["datatype"]) },
                    { "driver", Convert.ToBoolean(customClaims["driver"]) },
                    { "dynamics", Convert.ToBoolean(customClaims["dynamics"]) }
                },

                Features = new Dictionary<string, string[]>
                {
                    { "authentications", (string[]) customClaims["authentications"] },
                    { "encodings", (string[]) customClaims["encodings"] },
                    { "stores", (string[]) customClaims["stores"] },
                    { "routers", (string[]) customClaims["routers"] },
                    { "deployments", (string[]) customClaims["deployments"] },
                }
            };
        }

        private static LicenseQuota ForDemo()
        {
            return new()
            {
                Plan = ProductCatalog.TerminalPlanDemo,

                Limits = new Dictionary<string, object?>
                {
                    { "terminals", 1 },
                    { "commands", 25 },
                    { "inputs", 250 },
                    { "redistributions", 0 }
                },

                Switches = new Dictionary<string, bool>
            {
                { "datatype", true },
                { "driver", true },
                { "dynamics", true }
            },

                Features = new Dictionary<string, string[]>
                {
                    { "authentications", new[] { "msal", "oauth", "oidc", "none" } },
                    { "encodings", new [] { "ascii", "utf8", "utf16", "utf32" } },
                    { "stores", new [] { "memory" } },
                    { "routers", new [] { "console", "tcp", "udp", "grpc", "http" } },
                    { "deployments", new [] { "standard" } },
                }
            };
        }

        private static LicenseQuota ForEnterprise()
        {
            return new()
            {
                Plan = ProductCatalog.TerminalPlanEnterprise,

                Limits = new Dictionary<string, object?>
                {
                    { "terminals", 10 },
                    { "commands", 300 },
                    { "inputs", 6000 },
                    { "redistributions", 15000 }
                },

                Switches = new Dictionary<string, bool>
                {
                    { "datatype", true },
                    { "driver", true },
                    { "dynamics", true }
                },

                Features = new Dictionary<string, string[]>
                {
                    { "authentications", new[] { "msal", "oauth", "oidc", "none" } },
                    { "encodings", new [] { "ascii", "utf8", "utf16", "utf32" } },
                    { "stores", new [] { "memory", "custom" } },
                    { "routers", new [] { "console", "tcp", "udp", "grpc", "http", "custom" } },
                    { "deployments", new [] { "standard", "air_gapped" } },
                }
            };
        }

        private static LicenseQuota ForMicro()
        {
            return new()
            {
                Plan = ProductCatalog.TerminalPlanMicro,

                Limits = new Dictionary<string, object?>
                {
                    { "terminals", 3 },
                    { "commands", 50 },
                    { "inputs", 500 },
                    { "redistributions", 1000 }
                },

                Switches = new Dictionary<string, bool>
                {
                    { "datatype", true },
                    { "driver", false },
                    { "dynamics", false }
                },

                Features = new Dictionary<string, string[]>
                {
                    { "authentications", new[] {"msal", "none" } },
                    { "encodings", new [] { "ascii", "utf8", "utf16", "utf32" } },
                    { "stores", new [] { "memory" } },
                    { "routers", new [] { "console" } },
                    { "deployments", new [] { "standard" } },
                }
            };
        }

        private static LicenseQuota ForSmb()
        {
            return new()
            {
                Plan = ProductCatalog.TerminalPlanSmb,

                Limits = new Dictionary<string, object?>
                {
                    { "terminals", 5 },
                    { "commands", 100 },
                    { "inputs", 2000 },
                    { "redistributions", 5000 }
                },

                Switches = new Dictionary<string, bool>
                {
                    { "datatype", true },
                    { "driver", true },
                    { "dynamics", false }
                },

                Features = new Dictionary<string, string[]>
                {
                    { "authentications", new[] { "msal", "oauth", "oidc", "none" } },
                    { "encodings", new [] { "ascii", "utf8", "utf16", "utf32" } },
                    { "stores", new [] { "memory" } },
                    { "routers", new [] { "console", "tcp", "udp" } },
                    { "deployments", new [] { "standard" } },
                }
            };
        }

        private static LicenseQuota ForSolo()
        {
            return new()
            {
                Plan = ProductCatalog.TerminalPlanSolo,

                Limits = new Dictionary<string, object?>
                {
                    { "terminals", 1 },
                    { "commands", 25 },
                    { "inputs", 250 },
                    { "redistributions", 0 }
                },

                Switches = new Dictionary<string, bool>
                {
                    { "datatype", false },
                    { "driver", false },
                    { "dynamics", false }
                },

                Features = new Dictionary<string, string[]>
                {
                    { "authentications", new[] { "none" } },
                    { "encodings", new [] { "ascii", "utf8", "utf16", "utf32" } },
                    { "stores", new [] { "memory" } },
                    { "routers", new [] { "console" } },
                    { "deployments", new [] { "standard" } },
                }
            };
        }
    }
}
