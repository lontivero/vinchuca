using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using DreamBot.System;

namespace DreamBot.Network
{
    public class BotIdentifier
    {
        public static int Size = 0x10;
        public static BotIdentifier Unknown = new BotIdentifier(new byte[Size]);
 
        private readonly byte[] _internal;
        public static BotIdentifier Id;

        public BotIdentifier(byte[] data)
        {
            Debug.Assert(data.Length == Size);
            _internal = data;
        }

        static BotIdentifier()
        {
            var info = SystemInfo.GetSystemInfoSummary();
            var bytes = Encoding.ASCII.GetBytes(info);
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(bytes);
            Id = new BotIdentifier(hash);
        }

        public int GetPort()
        {
            var sum = 0;
            foreach (var b in _internal)
            {
                sum += b;
            }
            var port = 3000 + sum%1000;
            return port;
        }

        public override bool Equals(object obj)
        {
            var other = obj as BotIdentifier;
            if(other == null) return false;
            for (var i = 0; i < _internal.Length; i++)
            {
                if(_internal[i] != other._internal[i])
                    return false;
            }
            return true;
        }

        protected bool Equals(BotIdentifier other)
        {
            return Equals(_internal, other._internal);
        }

        public override int GetHashCode()
        {
            var arr1 = new byte[] {_internal[0], _internal[4], _internal[8], _internal[12]};
            var i1 = BitConverter.ToInt32(arr1, 0);

            return i1;
        }

        public byte[] ToByteArray()
        {
            var arr = new byte[Size];
            _internal.CopyTo(arr, 0);
            return arr;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var b in _internal )
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public static BotIdentifier Parse(string hex)
        {
            var len = hex.Length;
            var bytes = new byte[len / 2];
            for (var i = 0; i < len; i += 2)
            {
                bytes[i/2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return new BotIdentifier(bytes);
        }
    }
}
