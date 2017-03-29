using System;
using System.Collections.Generic;

namespace REPL
{
    public class VirtualConsole
    {
        private readonly int _top;
        private readonly char[] _buffer;
        private int _bufferPos;
        private static readonly object _sync = new object();

        public VirtualConsole(int top, int bottom)
        {
            _top = top;
            _buffer = new char[(bottom - top -1) * Console.BufferWidth];
            _bufferPos = 0;
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
            foreach (var c in str)
            {
                if (c == '\r') continue;
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
                Console.SetCursorPosition(0, _top);
                Console.Write(_buffer);
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
        public void HideCursor()
        {
            Console.CursorVisible = false;
        }
    }
}
