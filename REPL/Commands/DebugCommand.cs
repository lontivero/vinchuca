using System;
using System.Collections.Generic;
using Mono.Options;
using REPL.Debugging;
using Vinchuca;
using Vinchuca.Network.Protocol.Peers;

namespace REPL.Commands
{
    class DebugCommand : Command
    {
        private readonly Agent _agent;
        private readonly CommandLineReader _repl;
        public bool ShowHelp { get; set; }

        public DebugCommand(Agent agent, CommandLineReader repl)
            : base("debug", "Allows to perform diagnostic tasks")
        {
            _agent = agent;
            _repl = repl;
            Options = new OptionSet() {
                "usage: debug command",
                "",
                "Execute diagnostic commands.",
                "eg: debug get-peer-list",
                { "help|h|?","Show this message and exit.", v => ShowHelp = v != null }
            };
            _repl.AddAutocompletionWords("debug", "get-peer-list", "clear-peer-list");
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
                    _repl.Console.WriteLine("commands: Missing required argument `command`.");
                    _repl.Console.WriteLine("commands: Use `help debug` for details.");
                    return 1;
                }
                var cmd = extra[0];
                if (cmd == "get-peer-list")
                {
                    Dumper.Dump(_repl.Console, _agent.PeerList, new[] {
                        new Column<PeerInfo> { Title = "Bot ID",    Width = -54, m= info => info.ToString() },
                        new Column<PeerInfo> { Title = "Seen",      Width = -26, m = info => info.LastSeen.ToLocalTime() },
                        new Column<PeerInfo> { Title = "Rep",       Width =   4, m = info => info.Reputation },
                        new Column<PeerInfo> { Title = "SharedKey", Width =  10, m = info => Convert.ToBase64String(info.EncryptionKey).Substring(0, 8) }
                    });
                }
                else if(cmd == "clear-peer-list")
                {
                    _agent.PeerList.Clear();
                }
                return 0;
            }
            catch (Exception e)
            {
                // _repl.Console.WriteLine("commands: {0}", CommandDemo.Verbosity >= 1 ? e.ToString() : e.Message);
                return 1;
            }
        }
    }
}
