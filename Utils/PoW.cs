using System.Security.Cryptography;

namespace DreamBot.Utils
{
    static class PoW
    {
        public static bool IsEnough(byte[] payload, int offset, int count, int requiredWork)
        {
            var hash = SHA256.Create().ComputeHash(payload, offset, count);
            var val = (uint)(hash[0] << 24 | hash[1] << 16 | hash[2] << 8 | hash[3]);
            var difficulty = 0xffffffff >> requiredWork;
            return val < difficulty;
        }
    }
}
