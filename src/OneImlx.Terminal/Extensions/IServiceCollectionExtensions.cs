/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
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

namespace OneImlx.Terminal.Extensions
{
    /// <summary>
    /// Provides extension methods to register <c>OneImlx.Terminal</c> services with an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the <c>OneImlx.Terminal</c> services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ITerminalCommandStore"/>.</typeparam>
        /// <typeparam name="TText">The type implementing <see cref="ITerminalTextHandler"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="setupAction">A delegate to configure the <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        public static ITerminalBuilder AddTerminal<TStore, TText>(this IServiceCollection services, TText textHandler, Action<TerminalOptions> setupAction)
            where TStore : class, ITerminalCommandStore
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
        /// Adds the <c>OneImlx.Terminal</c> services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ITerminalCommandStore"/>.</typeparam>
        /// <typeparam name="TText">The type implementing <see cref="ITerminalTextHandler"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="configuration">The configuration to bind to <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        public static ITerminalBuilder AddTerminal<TStore, TText>(this IServiceCollection services, TText textHandler, IConfiguration configuration)
            where TStore : class, ITerminalCommandStore
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
        /// Adds the <c>OneImlx.Terminal</c> services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ITerminalCommandStore"/>.</typeparam>
        /// <typeparam name="TText">The type implementing <see cref="ITerminalTextHandler"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        public static ITerminalBuilder AddTerminal<TStore, TText>(this IServiceCollection services, TText textHandler)
            where TStore : class, ITerminalCommandStore
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
        /// Adds the default <c>OneImlx.Terminal</c> services for console based terminal applications.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ITerminalCommandStore"/>.</typeparam>
        /// <typeparam name="TText">The type implementing <see cref="ITerminalTextHandler"/>.</typeparam>
        /// <typeparam name="THelp">The type implementing <see cref="ITerminalHelpProvider"/>.</typeparam>
        /// <typeparam name="TException">The type implementing <see cref="ITerminalExceptionHandler"/>.</typeparam>
        /// <typeparam name="TConsole">The type implementing <see cref="ITerminalConsole"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="setupAction">A delegate to configure the <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        /// <remarks>
        /// The console refers to an abstraction of the system console. The console can be a system console, a web
        /// console, or a custom console.
        /// </remarks>
        public static ITerminalBuilder AddTerminalConsole<TStore, TText, THelp, TException, TConsole>(this IServiceCollection services, TText textHandler, Action<TerminalOptions> setupAction)
            where TStore : class, ITerminalCommandStore
            where TText : class, ITerminalTextHandler
            where THelp : class, ITerminalHelpProvider
            where TException : class, ITerminalExceptionHandler
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

            return services.AddTerminalDefault<TStore, TText, THelp, TException>(textHandler, setupAction)
                           .AddTerminalRouter<TerminalConsoleRouter, TerminalConsoleRouterContext>()
                           .AddConsole<TConsole>();
        }

        /// <summary>
        /// Adds the default <c>OneImlx.Terminal</c> services to build custom terminal interfaces.
        /// </summary>
        /// <typeparam name="TStore">The type implementing <see cref="ITerminalCommandStore"/>.</typeparam>
        /// <typeparam name="TText">The type implementing <see cref="ITerminalTextHandler"/>.</typeparam>
        /// <typeparam name="THelp">The type implementing <see cref="ITerminalHelpProvider"/>.</typeparam>
        /// <typeparam name="TException">The type implementing <see cref="ITerminalExceptionHandler"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="setupAction">A delegate to configure the <see cref="TerminalOptions"/>.</param>
        /// <returns>A <see cref="ITerminalBuilder"/> that can be used to further configure the terminal services.</returns>
        public static ITerminalBuilder AddTerminalDefault<TStore, TText, THelp, TException>(this IServiceCollection services, TText textHandler, Action<TerminalOptions> setupAction)
            where TStore : class, ITerminalCommandStore
            where TText : class, ITerminalTextHandler
            where THelp : class, ITerminalHelpProvider
            where TException : class, ITerminalExceptionHandler
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
                   .AddCommandRouter<CommandRouter, CommandHandler, CommandRuntime>()
                   .AddCommandParser<CommandParser, CommandRouteParser>()
                   .AddOptionChecker<DataTypeMapper<Option>, OptionChecker>()
                   .AddArgumentChecker<DataTypeMapper<Argument>, ArgumentChecker>()
                   .AddExceptionHandler<TException>()
                   .AddHelpProvider<THelp>();
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
    }
}
