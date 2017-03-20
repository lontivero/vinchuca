using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Vinchuca.REPL
{
    class CommandLine
    {
        private string _input = string.Empty;
        private int _pos;
        private int _cursorPos = Console.CursorLeft;
        private string _prompt;

        public int Padding
        {
            get { return _prompt.Length + 1; }
        }

        public int CursorPosition
        {
            get { return _cursorPos; }
            set
            {
                _cursorPos = value + Padding;
                Console.SetCursorPosition(_cursorPos, Console.CursorTop);
            }
        }


        public int Position
        {
            get { return _pos; }
            set
            {
                if (value != _pos)
                {
                    _pos = value > _input.Length ? _input.Length : value;
                    _pos = value < 0 ? 0 : _pos;
                    CursorPosition = Position;
                }
            }    
        }

        public string Input
        {
            get { return _input; }
            set
            {
                if (value != _input)
                {
                    _input = value;
                    if (_pos > _input.Length)
                        _pos = _input.Length;

                    CursorPosition = 0;
                    Console.Write(new string(' ', 60));
                    CursorPosition = 0;
                    Console.Write("{0}", Input);
                    CursorPosition = Position;
                }
            }
        }

        public void SetInput(string str)
        {
            Input = str;
            Position = str.Length;
        }

        private void Remove()
        {
            Input = Input.Remove(_pos, 1);
        }

        internal void Reset()
        {
            Input = string.Empty;
        }

        public bool Handle(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                    Position--;
                    break;
                case ConsoleKey.RightArrow:
                    Position++;
                    break;
                case ConsoleKey.Delete:
                    if (Position < Input.Length)
                    {
                        Remove();
                    }
                    break;
                case ConsoleKey.Backspace:
                    if (Position > 0)
                    {
                        Position--;
                        Remove();
                    }
                    break;
                case ConsoleKey.Escape:
                    Reset();
                    break;
                case ConsoleKey.Home:
                    Position = 0;
                    break;
                case ConsoleKey.Enter:
                    Console.SetCursorPosition(0, Console.CursorTop + 1);
                    Console.Write(_prompt + " ");
                    break;
                case ConsoleKey.End:
                    Position = Input.Length;
                    break;
                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        Input = Input.Insert(Position, key.KeyChar.ToString());
                        Position++;
                    }
                    break;
            }
            return true;
        }

        public void SetPrompt(string s)
        {
            _prompt = s;
        }
    }


    public class CommandLineReader
    {
        private readonly CommandHistory _history;
        private readonly CommandLine _commandLine;
        private readonly CommandCompletion _completion;

        public CommandLineReader()
        {
            _commandLine = new CommandLine();
            _commandLine.SetPrompt("$");
            _history = new CommandHistory(_commandLine);
            _completion = new CommandCompletion(_commandLine);
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
            Console.Write("$ ");
            while (true)
            {
                var key = Console.ReadKey(true);
                _commandLine.Handle(key);
                _history.Handle(key);
                _completion.Handle(key);
                if (key.Key == ConsoleKey.Enter)
                {
                    _commandLine.Reset();
                }
            }
        }
    }

    internal class CommandCompletion
    {
        private readonly List<string> _words;
        private readonly CommandLine _cmdLine;

        public CommandCompletion(CommandLine cmdLine)
        {
            _cmdLine = cmdLine;
            _words = new List<string>();
        }

        public void Handle(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Tab:
                    int ios;
                    do
                    {
                        ios = _cmdLine.Input.LastIndexOf(" ");
                    } while (ios > _cmdLine.Position);
                    var partialWord = ios < 0
                        ? _cmdLine.Input.Substring(0, _cmdLine.Position)
                        : _cmdLine.Input.Substring(ios + 1, _cmdLine.Position - ios - 1);

                    foreach (var word in _words)
                    {
                        if (word.StartsWith(partialWord, StringComparison.OrdinalIgnoreCase))
                        {
                            var prev = ios < 0 ? "" : _cmdLine.Input.Substring(0, ios) + " ";
                            var newInput = string.Format("{0}{1}", prev, word);
                            _cmdLine.SetInput(newInput);
                        }
                    }
                    break;

                case ConsoleKey.Enter:
                    var words = _cmdLine.Input.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in words)
                    {
                        if (!_words.Contains(word))
                        {
                            _words.Add(word);
                        }
                    }
                    break;
            }
        }
    }

    internal class CommandHistory
    {
        private readonly CommandLine _cmdLine;
        private List<string> _commands = new List<string>();
        private int _pos = -1;

        public CommandHistory(CommandLine cmdLine)
        {
            _cmdLine = cmdLine;
        }

        public void Add(string command)
        {
            if (!_commands.Contains(command))
            {
                _commands.Add(command);
                _pos++;
            }
        }

        public void Handle(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    _pos = (_commands.Count + _pos) % _commands.Count;
                    _cmdLine.SetInput(_commands[_pos--]);
                    break;
                case ConsoleKey.DownArrow:
                    _pos = (_pos+1) % _commands.Count;
                    _cmdLine.SetInput(_commands[_pos]);
                    break;
                case ConsoleKey.Enter:
                    Add(_cmdLine.Input);
                    break;
            }
        }
    }
}
