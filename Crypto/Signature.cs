using System;
using System.Security.Cryptography;
using DreamBot.Network;
using DreamBot.Utils;

namespace DreamBot.Crypto
{
    class Signature
    {
        private static readonly RSACryptoServiceProvider PubRsa = RsaUtils.LoadPublicKey(Convert.FromBase64String(@"MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDEuMdX757iaBkrxN51IQMILf+o7nJhfddEQ8gurVNYgoGxg19NZtrJaegAiv6lbFO4jhkKzLX9mHXZrvxL+UD5lr458BA8vwm+ws7lxu+10exq0XnsH26ekvBveDcbndhX+fbc34HhZlJCib6MKMzUGebwckGbZK3oz4WCZc3QoQIDAQAB"));

        public void Verify(byte[] buffer)
        {
            const int signatureLen = 0x80;
            var payloadLen = buffer.Length - signatureLen;
            var payload = new byte[payloadLen];
            var signature = new byte[signatureLen];
            Buffer.BlockCopy(buffer, 0, payload, 0, payloadLen);
            Buffer.BlockCopy(buffer, payloadLen, signature, 0, signatureLen);

            var success = PubRsa.VerifyData(payload, CryptoConfig.MapNameToOID("SHA512"), signature);
            if (!success)
            {
                throw new InvalidMessageException(2, "No valid signature");
            }            
        }

        public byte[] Sign(byte[] data, RSACryptoServiceProvider pk)
        {
            var signature = pk.SignData(data, 0, data.Length, CryptoConfig.MapNameToOID("SHA512"));
            return BufferUtils.Concat(data, signature);
        }
    }
}
