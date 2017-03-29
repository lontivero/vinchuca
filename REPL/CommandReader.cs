using System;

namespace REPL
{
    public class CommandLineReader
    {
        private readonly CommandHistory _history;
        private readonly CommandLine _commandLine;
        private readonly CommandCompletion _completion;
        public event EventHandler<CommandEventArgs> NewCommand;

        public CommandLineReader()
        {
            _commandLine = new CommandLine();
            _commandLine.SetPrompt("$");
            _history = new CommandHistory(_commandLine);
            _completion = new CommandCompletion(_commandLine);
            Clear();
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
            Console.Clear();
            Console.WriteLine(@"____   ____.__              .__                          ");
            Console.WriteLine(@"\   \ /   /|__| ____   ____ |  |__  __ __  ____ _____    ");
            Console.WriteLine(@" \   Y   / |  |/    \_/ ___\|  |  \|  |  \/ ___\\__  \   ");
            Console.WriteLine(@"  \     /  |  |   |  \  \___|   Y  \  |  |  \___ / __ \_ ");
            Console.WriteLine(@"   \___/   |__|___|  /\___  >___|  /____/ \___  >____  / ");
            Console.WriteLine(@"                   \/     \/     \/           \/     \/  ");
            Console.WriteLine(@"Management, debugging and control console 0.0.1");
            Console.WriteLine();
        }

        public void Run()
        {
            _commandLine.Prompt();
            while (true)
            {
                Console.CursorVisible = true;
                var key = Console.ReadKey(true);
                Console.CursorVisible = false;
                _commandLine.Handle(key);
                _history.Handle(key);
                _completion.Handle(key);
                if (key.Key == ConsoleKey.Enter)
                {
                    var cmdLine = _commandLine.Input;
                    Console.CursorTop++;
                    Console.CursorLeft=0;
                    Console.CursorLeft = 0;
                    if (NewCommand != null)
                        NewCommand(this, new CommandEventArgs(cmdLine));
                    Console.WriteLine();
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
