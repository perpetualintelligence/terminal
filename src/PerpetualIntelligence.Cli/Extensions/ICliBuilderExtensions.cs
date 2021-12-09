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
using PerpetualIntelligence.Cli.Commands.Mappers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Integration;
using System;

namespace PerpetualIntelligence.Cli.Extensions
{
    /// <summary>
    /// The <see cref="ICliBuilder"/> extension methods.
    /// </summary>
    public static class ICliBuilderExtensions
    {
        /// <summary>
        /// Adds the argument checker to the service..
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TMapper">The command extractor type.</typeparam>
        /// <typeparam name="TChecker">The command extractor type.</typeparam>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddArgumentChecker<TMapper, TChecker>(this ICliBuilder builder) where TMapper : class, IArgumentMapper where TChecker : class, IArgumentChecker
        {
            builder.Services.AddTransient<IArgumentMapper, TMapper>();
            builder.Services.AddTransient<IArgumentChecker, TChecker>();
            return builder;
        }

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
            builder.Services.AddTransient<TRunner>();

            // Add command checker
            builder.Services.AddTransient<TChecker>();

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
        /// Adds the required core services with transient scope.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ICliBuilder"/>.</returns>
        public static ICliBuilder AddRouting<TRouter, THandler>(this ICliBuilder builder) where TRouter : class, ICommandRouter where THandler : class, ICommandHandler
        {
            // Add command router
            builder.Services.AddTransient<ICommandRouter, TRouter>();

            // Add command handler
            builder.Services.AddTransient<ICommandHandler, THandler>();

            return builder;
        }
    }
}
