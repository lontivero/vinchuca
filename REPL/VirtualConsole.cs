using System;
using System.Collections.Generic;

namespace Vinchuca.REPL
{
    class ConsolesManager
    {
        public static readonly ConsolesManager Instance = new ConsolesManager();
        private List<VirtualConsole> _consoles = new List<VirtualConsole>();
        private int _focus = 0;

        private ConsolesManager()
        {
        }

        public void Register(VirtualConsole console)
        {
            _consoles.Add(console);
        }

        public void RestoreFocus()
        {
            var consoleFocus = _consoles[_focus];
            consoleFocus.SetCursorPosition(consoleFocus.CursorLeft, consoleFocus.CursorTop);
        }

        public void SetFocus(VirtualConsole console)
        {
            var i = 0;
            foreach (var virtualConsole in _consoles)
            {
                if (virtualConsole == console)
                {
                    _focus = i;
                    return;
                }
                i++;
            }
        }
    }

    public class VirtualConsole
    {
        private readonly int _top;
        private readonly char[] _buffer;
        private int _bufferPos;
        private static readonly object _sync = new object();

        public VirtualConsole(int top, int bottom)
        {
            _top = top;
            _buffer = new char[(bottom - top) * Console.BufferWidth];
            _bufferPos = 0;
            ConsolesManager.Instance.Register(this);
        }

        public int CursorTop
        {
            get { return _bufferPos / Console.BufferWidth; }
            set
            {
                var left = CursorLeft;
                _bufferPos = (value * Console.BufferWidth) + left;
                Console.SetCursorPosition(left, CursorTop);
            }
        }
        public int CursorLeft
        {
            get { return _bufferPos % Console.BufferWidth; }
            set
            {
                _bufferPos = (CursorTop * Console.BufferWidth) + value;
                Console.SetCursorPosition(CursorLeft, CursorTop);
            }
        }

        public void Write(string str)
        {
            str = str.Replace("\r", "");
            foreach (var c in str)
            {
                if (c == '\n')
                {
                    _bufferPos = ((_bufferPos / Console.BufferWidth) + 1) * Console.BufferWidth;
                    continue;
                }
                while(_bufferPos >= _buffer.Length) ScrollUp();
                _buffer[_bufferPos++] = c;
            }
            Redraw();
        }

        public void WriteLine(string str)
        {
            Write(str+"\n");
        }
        public void WriteLine()
        {
            Write("\n");
        }

        private void Redraw()
        {
            lock (_sync)
            {
                Console.CursorVisible = false;
                Console.SetCursorPosition(0, _top);
                Console.Write(_buffer);
                ConsolesManager.Instance.RestoreFocus();
                Console.CursorVisible = true;
            }
        }

        private void ScrollUp()
        {
            Buffer.BlockCopy(_buffer, sizeof(char)*Console.BufferWidth, _buffer, 0, sizeof(char)*(_buffer.Length - Console.BufferWidth));
            Array.Clear(_buffer, _buffer.Length - Console.BufferWidth, Console.BufferWidth);
            _bufferPos -= Console.BufferWidth;
            Redraw();
        }

        public ConsoleKeyInfo ReadKey(bool b)
        {
            return Console.ReadKey(b);
        }

        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _bufferPos = 0;
            Redraw();
        }

        public void SetCursorPosition(int left, int top)
        {
            CursorLeft = left;
            CursorTop = top;
        }

        public void ShowCursor()
        {
            Console.CursorVisible = true;
        }
    }
}
