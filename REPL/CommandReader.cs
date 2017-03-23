using System;

namespace Vinchuca.REPL
{
    public class CommandLineReader
    {
        private readonly CommandHistory _history;
        private readonly CommandLine _commandLine;
        private readonly CommandCompletion _completion;
        public event EventHandler<CommandEventArgs> NewCommand;
        private readonly VirtualConsole _virtualConsole;

        public CommandLineReader(VirtualConsole console)
        {
            _virtualConsole = console;
            _commandLine = new CommandLine(_virtualConsole);
            _commandLine.SetPrompt("$");
            _history = new CommandHistory(_commandLine);
            _completion = new CommandCompletion(_commandLine);
            Clear();
        }

        public VirtualConsole Console
        {
            get { return _virtualConsole; }
        }

        public void AddAutocompletionWords(params string[] words)
        {
            foreach (var word in words)
            {
                _completion.AddWord(word);
            }
        }

        public void Clear()
        {
            _virtualConsole.Clear();
            _virtualConsole.WriteLine(@"____   ____.__              .__                          ");
            _virtualConsole.WriteLine(@"\   \ /   /|__| ____   ____ |  |__  __ __  ____ _____    ");
            _virtualConsole.WriteLine(@" \   Y   / |  |/    \_/ ___\|  |  \|  |  \/ ___\\__  \   ");
            _virtualConsole.WriteLine(@"  \     /  |  |   |  \  \___|   Y  \  |  |  \___ / __ \_ ");
            _virtualConsole.WriteLine(@"   \___/   |__|___|  /\___  >___|  /____/ \___  >____  / ");
            _virtualConsole.WriteLine(@"                   \/     \/     \/           \/     \/  ");
            _virtualConsole.WriteLine(@"Management, debugging and control console 0.0.1");
            _virtualConsole.WriteLine();
        }

        public void Run()
        {
            _virtualConsole.Write("> ");
            while (true)
            {
                var key = _virtualConsole.ReadKey(true);
                _commandLine.Handle(key);
                _history.Handle(key);
                _completion.Handle(key);
                if (key.Key == ConsoleKey.Enter)
                {
                    var cmdLine = _commandLine.Input;
                    _virtualConsole.CursorTop++;
                    _virtualConsole.CursorLeft=0;
                    _commandLine.Reset();
                    _virtualConsole.CursorLeft = 0;
                    if (NewCommand != null)
                        NewCommand(this, new CommandEventArgs(cmdLine));
                    _virtualConsole.Write("\n");
                    _commandLine.Prompt();
                }
            }
        }
    }

    public class CommandEventArgs : EventArgs
    {
        public CommandEventArgs(string str)
        {
            Command = str;
        }

        public string Command { get; set; }
    }
}
