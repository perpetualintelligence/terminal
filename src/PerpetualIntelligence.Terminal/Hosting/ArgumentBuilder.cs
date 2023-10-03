/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/
/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Checkers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PerpetualIntelligence.Terminal.Hosting
{
    /// <summary>
    /// The default <see cref="IArgumentBuilder"/>.
    /// </summary>
    public sealed class ArgumentBuilder : IArgumentBuilder
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="commandBuilder">The <see cref="ICommandBuilder"/>.</param>
        public ArgumentBuilder(ICommandBuilder commandBuilder)
        {
            this.commandBuilder = commandBuilder;
            Services = new ServiceCollection();
        }

        /// <summary>
        /// The service collection.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Builds an <see cref="ArgumentBuilder"/> and adds it to the service collection.
        /// </summary>
        /// <returns></returns>
        public ICommandBuilder Add()
        {
            ServiceProvider lsp = Services.BuildServiceProvider();
            ArgumentDescriptor? argumentDescriptor = lsp.GetService<ArgumentDescriptor>() ?? throw new TerminalException(TerminalErrors.MissingArgument, "The argument builder is missing an argument descriptor.");

            commandBuilder.Services.AddSingleton(argumentDescriptor);

            // Does nothing for now.
            return commandBuilder;
        }

        private readonly ICommandBuilder commandBuilder;
    }
}