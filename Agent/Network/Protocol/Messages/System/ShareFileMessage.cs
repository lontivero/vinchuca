using System.IO;
using System.Net;

namespace Vinchuca.Network.Protocol.Messages.System
{
    public class ShareFileMessage : Message
    {
        public IPEndPoint Endpoint { get; set; }
        public string Path { get; set; }

        public override void EncodePayload(BinaryWriter w)
        {
            w.Write(Endpoint.Address.GetAddressBytes());
            w.Write((ushort)Endpoint.Port);
            w.Write(Path);
        }

        public override void DecodePayload(BinaryReader br)
        {
            Endpoint = new IPEndPoint(new IPAddress(br.ReadBytes(4)), br.ReadUInt16());
            Path = br.ReadString();
        }
    }
}