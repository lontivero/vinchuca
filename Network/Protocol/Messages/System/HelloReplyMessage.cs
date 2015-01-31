using System.Collections.Generic;
using System.IO;
using System.Net;
using DreamBot.Network.Protocol.Peers;

namespace DreamBot.Network.Protocol.Messages.System
{
    public class HelloReplyMessage : Message
    {
        public short BotVersion { get; set; }
        public short CfgVersion { get; set; }
        public PeerInfo[] Peers { get; set; }

        public override void EncodePayload(BinaryWriter w)
        {
            w.Write(BotVersion);
            w.Write(CfgVersion);
            w.Write(Peers.Length);
            foreach (var peer in Peers)
            {
                w.Write(peer.BotId.ToByteArray());
                w.Write(peer.EndPoint.Address.GetAddressBytes());
                w.Write(peer.EndPoint.Port);
            }
        }

        public override void DecodePayload(BinaryReader br)
        {
            BotVersion = br.ReadInt16();
            CfgVersion = br.ReadInt16();
            var len = br.ReadInt32();
            var peers = new List<PeerInfo>(len);
            for (int i = 0; i < len; i++)
            {
                var botId = new BotIdentifier(br.ReadBytes(BotIdentifier.Size));
                var ip = new IPAddress(br.ReadBytes(4));
                var port = br.ReadInt32();
                var peerInfo = new PeerInfo(botId, new IPEndPoint(ip, port));
                peers.Add(peerInfo);
            }
            Peers = peers.ToArray();
        }
    }
}