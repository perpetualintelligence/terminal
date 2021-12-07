/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.RequestHandlers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Integration.Configuration.Options;
using PerpetualIntelligence.Cli.Stores.InMemory;
using PerpetualIntelligence.Cli.Integration.Configuration;
using PerpetualIntelligence.Shared.Attributes;
using System;

namespace PerpetualIntelligence.Cli.Extensions
{
    /// <summary>
    /// The <see cref="ICliBuilder"/> extension methods.
    /// </summary>
    public static class ICliBuilderExtensions
    {
        /// <summary>
        /// Adds the required <see cref="CliOptions"/> with singleton scope.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddCliOptions(this ICliBuilder builder)
        {
            // Add options.
            builder.Services.AddOptions();
            builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<CliOptions>>().Value);
            return builder;
        }

        /// <summary>
        /// Adds the <c>cli</c> command identity as service.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="commandIdentity">The command identity.</param>
        /// <typeparam name="TRunner">The command runner type.</typeparam>
        /// <typeparam name="TChecker">The command checker type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddCommandIdentity<TRunner, TChecker>(this ICliBuilder builder, CommandIdentity commandIdentity) where TRunner : class, ICommandRunner where TChecker : class, ICommandChecker
        {
            if (commandIdentity.Runner != null && commandIdentity.Checker != null)
            {
                throw new InvalidOperationException("The command identity cannot specify runner or checker during explicit configuration.");
            }

            // Add the command identity as a singleton. Set the runner and checker as transient. These are internal fields.
            commandIdentity.Runner = typeof(TRunner);
            commandIdentity.Checker = typeof(TChecker);
            builder.Services.AddSingleton(commandIdentity);

            // Add command runner
            builder.Services.AddTransient<ICommandRunner, TRunner>();

            // Add command checker
            builder.Services.AddTransient<ICommandChecker, TChecker>();

            return builder;
        }

        /// <summary>
        /// Adds the <c>cli</c> command identity store to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TStore">The command identity store type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddCommandIdentityStore<TStore>(this ICliBuilder builder) where TStore : class, ICommandIdentityStore
        {
            // Add command extractor
            builder.Services.AddTransient<ICommandIdentityStore, TStore>();

            return builder;
        }

        /// <summary>
        /// Adds the required core services with transient scope.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddCore(this ICliBuilder builder)
        {
            // Add command router
            builder.Services.AddTransient<ICommandRouter, CommandRouter>();

            // Add command handler
            builder.Services.AddTransient<ICommandHandler, CommandHandler>();

            return builder;
        }

        /// <summary>
        /// Adds the required command extractor services with transient scope.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TCommand">The command extractor type.</typeparam>
        /// <typeparam name="TArgument">The argument extractor type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddExtractor<TCommand, TArgument>(this ICliBuilder builder) where TCommand : class, ICommandExtractor where TArgument : class, IArgumentExtractor
        {
            // Add command extractor
            builder.Services.AddTransient<ICommandExtractor, TCommand>();

            // Add argument extractor
            builder.Services.AddTransient<IArgumentExtractor, TArgument>();

            return builder;
        }

        /// <summary>
        /// Adds the <c>oneimlx</c> cli commands to the service collection.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        [Todo("Move this to OneImlx server")]
        internal static ICliBuilder AddOneImlxCommandIdentities(this ICliBuilder builder)
        {
            CommandIdentity map = new("urn:oneimlx:cli:map", "map", "map", new()
            {
                new ArgumentIdentity("r", System.ComponentModel.DataAnnotations.DataType.Text, true, "The root path for source projects."),
                new ArgumentIdentity("p", System.ComponentModel.DataAnnotations.DataType.Text, true, "The comma (,) separated source project names. Projects must organized be in the standard src and test hierarchy."),
                new ArgumentIdentity("c", System.ComponentModel.DataAnnotations.DataType.Text, true, "The configuration, Debug or Release.", new string[] { "Debug", "Release" }),
                new ArgumentIdentity("f", System.ComponentModel.DataAnnotations.DataType.Text, true, "The .NET framework identifier."),
                new ArgumentIdentity("o", System.ComponentModel.DataAnnotations.DataType.Text, true, "The mapping JSON file path.")
            });
            builder.AddCommandIdentity<MapCommandRunner, CommandChecker>(map);

            builder.AddCommandIdentityStore<InMemoryCommandIdentityStore>();

            return builder;
        }
    }
}
