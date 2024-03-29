/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Mappers;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using System;

namespace OneImlx.Terminal.Extensions
{
    /// <summary>
    /// Provides extension methods to register terminal services with an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the terminal services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ITerminalImmutableCommandStore"/>.</typeparam>
        /// <typeparam name="TText">The type implementing <see cref="ITerminalTextHandler"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="setupAction">A delegate to configure the <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        public static ITerminalBuilder AddTerminal<TStore, TText>(this IServiceCollection services, TText textHandler, Action<TerminalOptions> setupAction)
            where TStore : class, ITerminalImmutableCommandStore
            where TText : class, ITerminalTextHandler
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (textHandler is null)
            {
                throw new ArgumentNullException(nameof(textHandler));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.Configure(setupAction);
            return services.AddTerminal<TStore, TText>(textHandler);
        }

        /// <summary>
        /// Adds the terminal services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ITerminalImmutableCommandStore"/>.</typeparam>
        /// <typeparam name="TText">The type implementing <see cref="ITerminalTextHandler"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="configuration">The configuration to bind to <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        public static ITerminalBuilder AddTerminal<TStore, TText>(this IServiceCollection services, TText textHandler, IConfiguration configuration)
            where TStore : class, ITerminalImmutableCommandStore
            where TText : class, ITerminalTextHandler
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (textHandler is null)
            {
                throw new ArgumentNullException(nameof(textHandler));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.Configure<TerminalOptions>(configuration);
            return services.AddTerminal<TStore, TText>(textHandler);
        }

        /// <summary>
        /// Adds the terminal services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ITerminalImmutableCommandStore"/>.</typeparam>
        /// <typeparam name="TText">The type implementing <see cref="ITerminalTextHandler"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        public static ITerminalBuilder AddTerminal<TStore, TText>(this IServiceCollection services, TText textHandler)
            where TStore : class, ITerminalImmutableCommandStore
            where TText : class, ITerminalTextHandler
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (textHandler is null)
            {
                throw new ArgumentNullException(nameof(textHandler));
            }

            return services.CreateTerminalBuilder(textHandler)
                           .AddConfigurationOptions()
                           .AddCommandStore<TStore>()
                           .AddLicensing();
        }

        /// <summary>
        /// Creates a <see cref="ITerminalBuilder"/> for configuring terminal services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The global text handler.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        /// <remarks>
        /// This method is part of the terminal infrastructure and is not intended to be used directly from application code.
        /// </remarks>
        internal static ITerminalBuilder CreateTerminalBuilder(this IServiceCollection services, ITerminalTextHandler textHandler)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            TerminalBuilder tb = new(services, textHandler);
            tb.AddTextHandler(textHandler);
            return tb;
        }

        /// <summary>
        /// Adds the default terminal services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ITerminalImmutableCommandStore"/>.</typeparam>
        /// <typeparam name="TText">The type implementing <see cref="ITerminalTextHandler"/>.</typeparam>
        /// <typeparam name="THelp">The type implementing <see cref="ITerminalHelpProvider"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="setupAction">A delegate to configure the <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        public static ITerminalBuilder AddTerminalDefault<TStore, TText, THelp>(this IServiceCollection services, TText textHandler, Action<TerminalOptions> setupAction)
            where TStore : class, ITerminalImmutableCommandStore
            where TText : class, ITerminalTextHandler
            where THelp : class, ITerminalHelpProvider
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (textHandler is null)
            {
                throw new ArgumentNullException(nameof(textHandler));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            return services.AddTerminal<TStore, TText>(textHandler, setupAction)
                    .AddCommandRouter<CommandRouter, CommandHandler>()
                    .AddCommandParser<CommandParser, CommandRouteParser>()
                    .AddOptionChecker<DataTypeMapper<Option>, OptionChecker>()
                    .AddArgumentChecker<DataTypeMapper<Argument>, ArgumentChecker>()
                    .AddExceptionHandler<TerminalExceptionHandler>()
                    .AddHelpProvider<THelp>();
        }

        /// <summary>
        /// Adds the default terminal services for console applications to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ITerminalImmutableCommandStore"/>.</typeparam>
        /// <typeparam name="TText">The type implementing <see cref="ITerminalTextHandler"/>.</typeparam>
        /// <typeparam name="THelp">The type implementing <see cref="ITerminalHelpProvider"/>.</typeparam>
        /// <typeparam name="TConsole">The type implementing <see cref="ITerminalConsole"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="setupAction">A delegate to configure the <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        public static ITerminalBuilder AddTerminalConsole<TStore, TText, THelp, TConsole>(this IServiceCollection services, TText textHandler, Action<TerminalOptions> setupAction)
            where TStore : class, ITerminalImmutableCommandStore
            where TText : class, ITerminalTextHandler
            where THelp : class, ITerminalHelpProvider
            where TConsole : class, ITerminalConsole
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (textHandler is null)
            {
                throw new ArgumentNullException(nameof(textHandler));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            return services.AddTerminalDefault<TStore, TText, THelp>(textHandler, setupAction)
                           .AddTerminalRouter<TerminalConsoleRouter, TerminalConsoleRouterContext>()
                           .AddConsole<TConsole>();
        }
    }
}