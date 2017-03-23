using System;
using System.Collections.Generic;

namespace Vinchuca.REPL
{
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
                    if (_commands.Count == 0) return;
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