using System.Text;

namespace DreamBot.Crypto
{
    class Mod2 : Cipher
    {
        private readonly byte[] _key;

        public Mod2(byte[] key)
        {
            _key = key;
        }

        public override void Decrypt(byte[] src, int srcOffset, byte[] dest, int destOffset, int count)
        {
            Encrypt(src, srcOffset, dest, destOffset, count);
        }

        public override void Encrypt(byte[] src, int srcOffset, byte[] dest, int destOffset, int count)
        {
            for (var i = 0; i < count; i++)
                dest[i + destOffset] = (byte)(src[i + srcOffset] ^ _key[(i % _key.Length)]);
        }
    }
}
