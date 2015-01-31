using System.IO;
using System.Net;

namespace DreamBot.Network.Protocol.Messages
{
    public class BotHeader
    {
        public static int Size = 32;
        public short Padding { get; set; }
        public short MessageId { get; set; }
        public short PayloadSize { get; set; }
        public short Ttl { get; set; }
        public ulong CorrelationId { get; set; }
        public BotIdentifier BotId { get; set; }

        public byte[] Encode()
        {
            var buffer = new byte[Size];
            using (var w = new BinaryWriter(new MemoryStream(buffer)))
            {
                w.Write(Padding);
                w.Write(MessageId);
                w.Write(PayloadSize);
                w.Write(CorrelationId);
                w.Write(Ttl);
                w.Write(BotId.ToByteArray());
            }
            return buffer;
        }

        public static BotHeader Decode(byte[] data)
        {
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                return new BotHeader {
                    Padding = br.ReadInt16(),
                    MessageId = br.ReadInt16(),
                    PayloadSize = br.ReadInt16(),
                    CorrelationId = br.ReadUInt64(),
                    Ttl = br.ReadInt16(),
                    BotId = new BotIdentifier(br.ReadBytes(BotIdentifier.Size))
                };
            }
        }

        public IPEndPoint EndPoint { get; set; }
    }
}
