/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Configuration.Options;
using System.Linq;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalHelpProvider"/> that logs the command help using <see cref="ITerminalConsole"/>.
    /// </summary>
    public sealed class TerminalConsoleHelpProvider : ITerminalHelpProvider
    {
        private readonly TerminalOptions terminalOptions;
        private readonly ITerminalConsole terminalConsole;

        /// <summary>
        /// Initializes new instance.
        /// </summary>
        public TerminalConsoleHelpProvider(TerminalOptions terminalOptions, ITerminalConsole terminalConsole)
        {
            this.terminalOptions = terminalOptions;
            this.terminalConsole = terminalConsole ?? throw new System.ArgumentNullException(nameof(terminalConsole));
        }

        /// <inheritdoc/>
        public async Task ProvideHelpAsync(TerminalHelpProviderContext context)
        {
            int indent = 2;
            await terminalConsole.WriteLineAsync("Command:");
            await terminalConsole.WriteLineAsync(string.Format("{0}{1} ({2}) {3}", new string(' ', indent), context.Command.Id, context.Command.Name, context.Command.Descriptor.Type));
            await terminalConsole.WriteLineAsync(string.Format("{0}{1}", new string(' ', indent * 2), context.Command.Description));

            if (context.Command.Descriptor.ArgumentDescriptors != null)
            {
                indent = 2;
                await terminalConsole.WriteLineAsync("Arguments:");
                foreach (ArgumentDescriptor argument in context.Command.Descriptor.ArgumentDescriptors)
                {
                    await terminalConsole.WriteLineAsync(string.Format("{0}{1} <{2}>", new string(' ', indent), argument.Id, argument.DataType));
                    await terminalConsole.WriteLineAsync(string.Format("{0}{1}", new string(' ', indent * 2), argument.Description));
                }
            }

            if (context.Command.Descriptor.OptionDescriptors != null)
            {
                indent = 2;
                await terminalConsole.WriteLineAsync("Options:");
                foreach (OptionDescriptor option in context.Command.Descriptor.OptionDescriptors.Values.Distinct())
                {
                    if (option.Alias != null)
                    {
                        await terminalConsole.WriteLineAsync(string.Format("{0}{1}{2}, {3}{4} <{5}>", new string(' ', indent), terminalOptions.Parser.OptionPrefix, option.Id, terminalOptions.Parser.OptionAliasPrefix, option.Alias, option.DataType));
                    }
                    else
                    {
                        await terminalConsole.WriteLineAsync(string.Format("{0}{1}{2} <{3}>", new string(' ', indent), terminalOptions.Parser.OptionPrefix, option.Id, option.DataType));
                    }

                    await terminalConsole.WriteLineAsync(string.Format("{0}{1}", new string(' ', indent * 2), option.Description));
                }
            }
        }
    }
}