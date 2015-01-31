using System.IO;

namespace DreamBot.Network.Protocol.Messages
{
    public abstract class Message
    {
        public virtual byte[] Encode()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var w = new BinaryWriter(memoryStream))
                {
                    EncodePayload(w);
                }
                return memoryStream.ToArray();
            }
        }

        public virtual void Decode(BinaryReader br)
        {
            DecodePayload(br);
        }


        public virtual void EncodePayload(BinaryWriter w)
        {
        }

        public virtual void DecodePayload(BinaryReader br)
        {
        }
    }
}
