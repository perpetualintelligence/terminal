/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Terminal.Commands.Parsers;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Mappers;
using PerpetualIntelligence.Terminal.Commands.Providers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Hosting;
using PerpetualIntelligence.Terminal.Runtime;
using PerpetualIntelligence.Terminal.Stores;
using System;

namespace PerpetualIntelligence.Terminal.Extensions
{
    /// <summary>
    /// Provides extension methods to register terminal services with an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the terminal services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ICommandStore"/>.</typeparam>
        /// <typeparam name="TText">The type implementing <see cref="ITextHandler"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="setupAction">A delegate to configure the <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        public static ITerminalBuilder AddTerminal<TStore, TText>(this IServiceCollection services, Action<TerminalOptions> setupAction)
            where TStore : class, ICommandStore
            where TText : class, ITextHandler
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.Configure(setupAction);
            return services.AddTerminal<TStore, TText>();
        }

        /// <summary>
        /// Adds the terminal services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ICommandStore"/>.</typeparam>
        /// <typeparam name="TText">The type implementing <see cref="ITextHandler"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configuration">The configuration to bind to <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        public static ITerminalBuilder AddTerminal<TStore, TText>(this IServiceCollection services, IConfiguration configuration)
            where TStore : class, ICommandStore
            where TText : class, ITextHandler
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.Configure<TerminalOptions>(configuration);
            return services.AddTerminal<TStore, TText>();
        }

        /// <summary>
        /// Adds the terminal services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ICommandStore"/>.</typeparam>
        /// <typeparam name="TText">The type implementing <see cref="ITextHandler"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        public static ITerminalBuilder AddTerminal<TStore, TText>(this IServiceCollection services)
            where TStore : class, ICommandStore
            where TText : class, ITextHandler
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.CreateTerminalBuilder()
                           .AddConfigurationOptions()
                           .AddCommandStore<TStore>()
                           .AddTextHandler<TText>()
                           .AddLicensing();
        }

        /// <summary>
        /// Creates a <see cref="ITerminalBuilder"/> for configuring terminal services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        /// <remarks>
        /// This method is part of the terminal infrastructure and is not intended to be used directly from application code.
        /// </remarks>
        public static ITerminalBuilder CreateTerminalBuilder(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return new TerminalBuilder(services);
        }

        /// <summary>
        /// Adds the default terminal services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ICommandStore"/>.</typeparam>
        /// <typeparam name="TText">The type implementing <see cref="ITextHandler"/>.</typeparam>
        /// <typeparam name="THelp">The type implementing <see cref="IHelpProvider"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="setupAction">A delegate to configure the <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        public static ITerminalBuilder AddTerminalDefault<TStore, TText, THelp>(this IServiceCollection services, Action<TerminalOptions> setupAction)
            where TStore : class, ICommandStore
            where TText : class, ITextHandler
            where THelp : class, IHelpProvider
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            return services.AddTerminal<TStore, TText>(setupAction)
                    .AddCommandRouter<CommandRouter, CommandHandler>()
                    .AddCommandParser<CommandParser, CommandRouteParser>()
                    .AddOptionChecker<DataTypeMapper<Option>, OptionChecker>()
                    .AddArgumentChecker<DataTypeMapper<Argument>, ArgumentChecker>()
                    .AddExceptionHandler<ExceptionHandler>()
                    .AddHelpProvider<THelp>();
        }

        /// <summary>
        /// Adds the default terminal services for console applications to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ICommandStore"/>.</typeparam>
        /// <typeparam name="TText">The type implementing <see cref="ITextHandler"/>.</typeparam>
        /// <typeparam name="THelp">The type implementing <see cref="IHelpProvider"/>.</typeparam>
        /// <typeparam name="TConsole">The type implementing <see cref="ITerminalConsole"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="setupAction">A delegate to configure the <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        public static ITerminalBuilder AddTerminalConsole<TStore, TText, THelp, TConsole>(this IServiceCollection services, Action<TerminalOptions> setupAction)
            where TStore : class, ICommandStore
            where TText : class, ITextHandler
            where THelp : class, IHelpProvider
            where TConsole : class, ITerminalConsole
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            return services.AddTerminalDefault<TStore, TText, THelp>(setupAction)
                           .AddRouting<TerminalConsoleRouting, TerminalConsoleRoutingContext>()
                           .AddConsole<TConsole>();
        }
    }
}