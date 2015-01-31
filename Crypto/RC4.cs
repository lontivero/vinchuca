using System.Collections.Generic;
using System.Security.Cryptography;

namespace DreamBot.Crypto
{
    public class Rc4 : Cipher
    {
        private static readonly RandomNumberGenerator Random = new RNGCryptoServiceProvider();

        private readonly byte[] _s = new byte[256];
        private int _x;
        private int _y;

        public Rc4(IList<byte> key)
        {
            for (var i = 0; i < _s.Length; i++)
                _s[i] = (byte)i;

            for (var i = 0; i <= 255; i++)
            {
                _x = (_x + _s[i] + key[i % key.Count]) % 256;
                var c = _s[_x];
                _s[_x] = _s[i];
                _s[i] = c;
            }

            _x = 0;

            var wasteBuffer = new byte[1024];
            Random.GetBytes(wasteBuffer);
            Encrypt(wasteBuffer);
        }

        public override void Decrypt(byte[] src, int srcOffset, byte[] dest, int destOffset, int count)
        {
            Encrypt(src, srcOffset, dest, destOffset, count);
        }

        public override void Encrypt(byte[] src, int srcOffset, byte[] dest, int destOffset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                _x = (_x + 1) & 0xFF;
                _y = (_y + _s[_x]) & 0xFF;

                var c = _s[_y];
                _s[_y] = _s[_x];
                _s[_x] = c;

                dest[i + destOffset] = (byte)(src[i + srcOffset] ^ (_s[(_s[_x] + _s[_y]) & 0xFF]));
            }
        }
    }
}