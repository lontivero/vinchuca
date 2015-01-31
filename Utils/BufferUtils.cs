using System;

namespace DreamBot.Utils
{
    static class BufferUtils
    {
        public static byte[] Concat(byte[] b1, byte[] b2)
        {
            var buffer = new byte[b1.Length + b2.Length];
            Buffer.BlockCopy(b1, 0, buffer, 0, b1.Length);
            Buffer.BlockCopy(b2, 0, buffer, b1.Length, b2.Length);
            return buffer;
        }
    }
}
