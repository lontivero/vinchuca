using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DreamBot.Crypto;

namespace DreamBot.Network.Protocol.Messages.System
{
    public class HelloMessage : Message
    {
        public byte[] PublicKey { get; set; }

        public HelloMessage()
        {
            PublicKey = BotIdentifier.PublicKey;
        }

        public override void EncodePayload(BinaryWriter w)
        {
            w.Write((short) PublicKey.Length);
            w.Write(PublicKey);
        }

        public override void DecodePayload(BinaryReader br)
        {
            var len = br.ReadInt16();
            PublicKey = br.ReadBytes(len);
        }
    }
}