using System;
using System.Collections.Generic;
using Mono.Options;

namespace Vinchuca.REPL
{
    class DebugCommand : Command
    {
        private readonly Agent _agent;
        public bool ShowHelp { get; set; }

        public DebugCommand(Agent agent)
            : base("debug", "Allows to perform diagnostic tasks")
        {
            _agent = agent;
            Options = new OptionSet() {
            "usage: debug command",
            "",
            "Execute diagnostic commands.",
            "eg: debug get-peer-list",
            { "help|h|?","Show this message and exit.",
              v => ShowHelp = v != null },
        };
        }

        public override int Invoke(IEnumerable<string> args)
        {
            try
            {
                var extra = Options.Parse(args);
                if (ShowHelp)
                {
                    Options.WriteOptionDescriptions(CommandSet.Out);
                    return 0;
                }
                if (extra.Count == 0)
                {
                    Console.Error.WriteLine("commands: Missing required argument `command`.");
                    Console.Error.WriteLine("commands: Use `help debug` for details.");
                    return 1;
                }
                var cmd = extra[0];
                if (cmd == "get-peer-list")
                {
                    _agent.PeerList.Dump();
                }
                else if(cmd == "clear-peer-list")
                {
                    _agent.PeerList.Clear();
                }
                return 0;
            }
            catch (Exception e)
            {
//                Console.Error.WriteLine("commands: {0}", CommandDemo.Verbosity >= 1 ? e.ToString() : e.Message);
                return 1;
            }
        }
    }
}
