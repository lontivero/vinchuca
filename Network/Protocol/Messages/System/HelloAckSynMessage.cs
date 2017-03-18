using System.IO;

namespace Vinchuca.Network.Protocol.Messages.System
{
    public class HelloAckSynMessage : Message
    {
        public short BotVersion { get; set; }
        public short CfgVersion { get; set; }
        public byte[] PublicKey { get; set; }

        public override void EncodePayload(BinaryWriter w)
        {
            w.Write(BotVersion);
            w.Write(CfgVersion);
            w.Write((short)PublicKey.Length);
            w.Write(PublicKey);
        }

        public override void DecodePayload(BinaryReader br)
        {
            BotVersion = br.ReadInt16();
            CfgVersion = br.ReadInt16();
            var lenpk = br.ReadInt16();
            PublicKey = br.ReadBytes(lenpk);
        }
    }
}