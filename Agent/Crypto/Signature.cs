using System;
using System.Security.Cryptography;
using Vinchuca.Network;
using Vinchuca.Utils;

namespace Vinchuca.Crypto
{
    class Signature
    {
        internal const int Lenght = 0x80;

        private static readonly RSACryptoServiceProvider PubRsa = RsaUtils.LoadPublicKey(Convert.FromBase64String(@"MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDEuMdX757iaBkrxN51IQMILf+o7nJhfddEQ8gurVNYgoGxg19NZtrJaegAiv6lbFO4jhkKzLX9mHXZrvxL+UD5lr458BA8vwm+ws7lxu+10exq0XnsH26ekvBveDcbndhX+fbc34HhZlJCib6MKMzUGebwckGbZK3oz4WCZc3QoQIDAQAB"));

        public void Verify(byte[] buffer, byte[] signature)
        {
            var success = PubRsa.VerifyData(buffer, CryptoConfig.MapNameToOID("SHA1"), signature);
            if (!success)
            {
                throw new InvalidMessageException(2, "No valid signature");
            }
        }

        public void Verify(byte[] buffer)
        {
            var payloadLen = buffer.Length - Lenght;
            var payload = new byte[payloadLen];
            var signature = new byte[Lenght];
            Buffer.BlockCopy(buffer, 0, payload, 0, payloadLen);
            Buffer.BlockCopy(buffer, payloadLen, signature, 0, Lenght);
            Verify(payload, signature);
        }

        public byte[] Sign(byte[] data, RSACryptoServiceProvider pk)
        {
            var signature = pk.SignData(data, 0, data.Length, CryptoConfig.MapNameToOID("SHA1"));
            return BufferUtils.Concat(data, signature);
        }
    }
}
