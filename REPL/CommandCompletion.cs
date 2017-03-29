using System;
using System.Collections.Generic;

namespace REPL
{
    internal class CommandCompletion
    {
        private readonly List<string> _words;
        private readonly CommandLine _cmdLine;
        private static readonly char[] Separators = new[] {' ', ':', '=', '@'};

        public CommandCompletion(CommandLine cmdLine)
        {
            _cmdLine = cmdLine;
            _words = new List<string>();
        }

        public void AddWord(string word)
        {
            _words.Add(word);    
        }

        public void Handle(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Tab:
                    int ios;
                    do
                    {
                        ios = _cmdLine.Input.LastIndexOfAny(Separators);
                    } while (ios > _cmdLine.Position);
                    var partialWord = ios < 0
                        ? _cmdLine.Input.Substring(0, _cmdLine.Position)
                        : _cmdLine.Input.Substring(ios + 1, _cmdLine.Position - ios - 1);
                    var sep = ios < 0
                        ? ""
                        : _cmdLine.Input[ios].ToString();
                    foreach (var word in _words)
                    {
                        if (word.StartsWith(partialWord, StringComparison.OrdinalIgnoreCase))
                        {
                            var prev = ios < 0 ? "" : _cmdLine.Input.Substring(0, ios) + sep;
                            var newInput = string.Format("{0}{1}", prev, word);
                            _cmdLine.SetInput(newInput);
                            break;
                        }
                    }
                    break;

                case ConsoleKey.Enter:
                    var words = _cmdLine.Input.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
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
}