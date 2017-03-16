using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Vinchuca.Crypto
{
    class Aes
    {
        private static readonly byte[] InitialVectorBytes = Encoding.ASCII.GetBytes("OhRna+3m*aze01xZ");

        public static byte[] Encrypt(byte[] message, int offset, int count, byte[] key)
        {
            var symmetricKey = new RijndaelManaged {Mode = CipherMode.CBC};
            byte[] cipherTextBytes;

            using (var encryptor = symmetricKey.CreateEncryptor(key, InitialVectorBytes))
            {
                using (var memStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(message, offset, count);
                        cryptoStream.FlushFinalBlock();
                        cipherTextBytes = memStream.ToArray();
                        memStream.Close();
                        cryptoStream.Close();
                    }
                }
            }
            symmetricKey.Clear();
            return cipherTextBytes;
        }

        public static byte[] Decrypt(byte[] cipherTextBytes, int offset, int count, byte[] key)
        {
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };
            var plainTextBytes = new byte[count];
            var byteCount = 0;
            using (var decryptor = symmetricKey.CreateDecryptor(key, InitialVectorBytes))
            {
                using (var memStream = new MemoryStream(cipherTextBytes, offset, count))
                {
                    using (var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read))
                    {
                        byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                        memStream.Close();
                        cryptoStream.Close();
                    }
                }
            }
            symmetricKey.Clear();
            var ret = new byte[byteCount];
            Buffer.BlockCopy(plainTextBytes, 0, ret, 0, byteCount);
            return ret;
        }
    }
}
