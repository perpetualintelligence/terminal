
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneImlx.Terminal.Apps.Checkers;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;

namespace OneImlx.Terminal.Apps.Runners
{
    [CommandDescriptor("cmd3", "Command 3", "Command3 description.", Commands.CommandType.SubCommand, Commands.CommandFlags.Secured)]
    [CommandChecker(typeof(Cmd3CommandChecker))]
    internal class Cmd3AuthRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public override Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            throw new NotImplementedException();
        }
    }
}
