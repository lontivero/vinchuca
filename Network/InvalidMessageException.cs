using System;

namespace DreamBot.Network
{
    internal class InvalidMessageException : Exception
    {
        private readonly int _code;
        private readonly string _magicNumber;

        public InvalidMessageException(int code, string message)
        {
            _code = code;
            _magicNumber = message;
        }

        public int Code
        {
            get { return _code; }
        }

        public string MagicNumber
        {
            get { return _magicNumber; }
        }
    }
}