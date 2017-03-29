using System;

namespace REPL
{
    internal class CommandLine
    {
        private string _input = string.Empty;
        private int _pos;
        private int _cursorPos = Console.CursorLeft;
        private string _prompt;

        public CommandLine()
        {
        }

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
                Console.CursorLeft = _cursorPos;
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
                    Console.Write(new string(' ', Console.BufferWidth-Padding-2));
                    CursorPosition = 0;
                    Console.Write(Input);
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

        public void Prompt()
        {
            Console.CursorLeft=0;
            Console.Write("> ");
            Reset();
            CursorPosition = 0;
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
}