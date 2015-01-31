using System;

namespace DreamBot.Utils
{
    static class RandomUtils
    {
        private static readonly Random Rand = new Random();
        internal static ulong NextCorrelationId()
        {
            var buf = new byte[8];
            Rand.NextBytes(buf);
            return BitConverter.ToUInt64(buf, 0);
        }

        internal static byte[] NextPadding()
        {
            var padLen = Rand.Next(2, 128);
            var buf = new byte[padLen];
            Rand.NextBytes(buf);
            return buf;
        }

        internal static short NextTtl()
        {
            var buf = new byte[2];
            Rand.NextBytes(buf);
            return BitConverter.ToInt16(buf, 0);
        }
    }
}
